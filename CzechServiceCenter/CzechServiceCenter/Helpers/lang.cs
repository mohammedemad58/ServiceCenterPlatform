using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace CzechServiceCenter
{
    /// <summary>
    /// English / Arabic switch. The language is kept in the "csc_lang" cookie (set by ?lang=ar or ?lang=en).
    /// Static text in pages is translated by LangFilter using the dictionary below; text built in C# goes through Lang.T().
    /// Names of brands, services, parts and branches are stored in the database (NameAr / DescriptionAr columns).
    /// </summary>
    public static class Lang
    {
        public const string CookieName = "csc_lang";

        public static bool IsAr
        {
            get
            {
                var c = HttpContext.Current;
                if (c == null) return false;
                var o = c.Items["csc_lang"] as string;
                if (o != null) return o == "ar";
                var ck = c.Request.Cookies[CookieName];
                return ck != null && ck.Value == "ar";
            }
        }

        public static string Code { get { return IsAr ? "ar" : "en"; } }
        public static string Dir { get { return IsAr ? "rtl" : "ltr"; } }

        public static void Set(string code)
        {
            if (code != "ar" && code != "en") return;
            var c = HttpContext.Current;
            c.Items["csc_lang"] = code;
            var ck = new HttpCookie(CookieName, code);
            ck.Expires = DateTime.Now.AddYears(1);
            ck.HttpOnly = true;
            ck.Path = "/";
            c.Response.Cookies.Add(ck);
        }

        static string Norm(string s)
        {
            return Regex.Replace(s ?? "", @"\s+", " ").Trim();
        }

        /// <summary>Arabic text for an English UI string when the site is in Arabic, otherwise the string itself.</summary>
        public static string T(string en)
        {
            if (!IsAr || string.IsNullOrEmpty(en)) return en;
            string ar;
            return Map.TryGetValue(Norm(en), out ar) ? ar : en;
        }

        /// <summary>Translated template filled with values: Lang.T("Page {0} of {1}", 2, 5).</summary>
        public static string T(string template, params object[] args)
        {
            return string.Format(T(template), args);
        }

        /// <summary>SQL expression that returns the Arabic column when the site is in Arabic and it is filled in (e.g. Col("s","Name")).</summary>
        public static string Col(string alias, string column)
        {
            string p = string.IsNullOrEmpty(alias) ? "" : alias + ".";
            if (!IsAr) return p + column;
            return "COALESCE(NULLIF(" + p + column + "Ar,''), " + p + column + ")";
        }

        // ---------- formatting ----------
        static readonly string[] ArMonths = { "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };
        static readonly string[] ArDays = { "الأحد", "الاثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت" };

        public static string Date(DateTime d)
        {
            if (!IsAr) return d.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
            return d.Day.ToString("00") + " " + ArMonths[d.Month - 1] + " " + d.Year;
        }

        public static string DayName(DayOfWeek d)
        {
            return IsAr ? ArDays[(int)d] : d.ToString();
        }

        public static string Money(decimal v)
        {
            return v.ToString("N2", CultureInfo.InvariantCulture) + (IsAr ? " جنيه" : " EGP");
        }

        public static string Km(int v)
        {
            return v.ToString("N0", CultureInfo.InvariantCulture) + (IsAr ? " كم" : " km");
        }

        // ---------- translating a finished HTML page ----------
        static readonly Regex Protected = new Regex(@"(<script\b[^>]*>.*?</script>|<style\b[^>]*>.*?</style>|<textarea\b[^>]*>.*?</textarea>|<!--.*?-->)",
            RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex TextNode = new Regex(@">([^<>]+)(?=<)", RegexOptions.Compiled);
        static readonly Regex Attr = new Regex(@"\b(placeholder|aria-label|title|alt|data-confirm)=""([^""]*)""", RegexOptions.Compiled);
        static readonly Regex TitleTag = new Regex(@"(<title>)(.*?)(</title>)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        static string Lookup(string raw)
        {
            string ar;
            return Map.TryGetValue(Norm(HttpUtility.HtmlDecode(raw)), out ar) ? ar : null;
        }

        public static string Translate(string html)
        {
            html = TitleTag.Replace(html, delegate (Match m)
            {
                var parts = HttpUtility.HtmlDecode(m.Groups[2].Value).Split(new[] { " | " }, StringSplitOptions.None);
                for (int i = 0; i < parts.Length; i++)
                {
                    string ar; if (Map.TryGetValue(Norm(parts[i]), out ar)) parts[i] = ar;
                }
                return m.Groups[1].Value + HttpUtility.HtmlEncode(string.Join(" | ", parts)) + m.Groups[3].Value;
            });

            var sb = new StringBuilder(html.Length + 4096);
            int pos = 0;
            foreach (Match p in Protected.Matches(html))
            {
                sb.Append(TranslateChunk(html.Substring(pos, p.Index - pos)));
                sb.Append(p.Value);
                pos = p.Index + p.Length;
            }
            sb.Append(TranslateChunk(html.Substring(pos)));
            return sb.ToString();
        }

        static string TranslateChunk(string s)
        {
            s = TextNode.Replace(s, delegate (Match m)
            {
                string t = m.Groups[1].Value;
                string core = t.Trim();
                if (core.Length == 0) return m.Value;
                string ar = Lookup(core);
                if (ar == null) return m.Value;
                int lead = t.IndexOf(core, StringComparison.Ordinal);
                return ">" + t.Substring(0, lead) + HttpUtility.HtmlEncode(ar) + t.Substring(lead + core.Length);
            });
            return Attr.Replace(s, delegate (Match m)
            {
                string ar = Lookup(m.Groups[2].Value);
                return ar == null ? m.Value : m.Groups[1].Value + "=\"" + HttpUtility.HtmlAttributeEncode(ar) + "\"";
            });
        }

        static readonly Dictionary<string, string> Map = Build();

        static Dictionary<string, string> Build()
        {
            var M = new Dictionary<string, string>(StringComparer.Ordinal);
            M["Home"] = "الرئيسية";
            M["About Us"] = "من نحن";
            M["Our Brands"] = "ماركاتنا";
            M["Genuine Parts"] = "قطع الغيار الأصلية";
            M["Book Service"] = "احجز صيانة";
            M["Contact"] = "اتصل بنا";
            M["Admin"] = "الإدارة";
            M["My Account"] = "حسابي";
            M["Log in"] = "تسجيل الدخول";
            M["Log out"] = "تسجيل الخروج";
            M["Menu"] = "القائمة";
            M["Main"] = "القائمة الرئيسية";
            M["CZECH SERVICE CENTER"] = "المركز التشيكي للصيانة";
            M["Czech Service Center"] = "المركز التشيكي للصيانة";
            M["SERVICE • GENUINE PARTS • TRUST"] = "خدمة • قطع أصلية • ثقة";
            M["Czech Service Center home"] = "المركز التشيكي للصيانة - الرئيسية";
            M["Follow Us"] = "تابعنا";
            M["Contact Us"] = "تواصل معنا";
            M["Facebook"] = "فيسبوك";
            M["Instagram"] = "إنستجرام";
            M["YouTube"] = "يوتيوب";
            M["LinkedIn"] = "لينكد إن";
            M["© {0} Czech Service Center. All rights reserved."] = "© {0} المركز التشيكي للصيانة. جميع الحقوق محفوظة.";
            M["Audi"] = "أودي";
            M["Škoda"] = "سكودا";
            M["ŠKODA"] = "سكودا";
            M["SEAT"] = "سيات";
            M["Volkswagen"] = "فولكس فاجن";
            M["CUPRA"] = "كوبرا";
            M["Your Trusted"] = "شريكك الموثوق";
            M["Service Partner"] = "للصيانة والخدمة";
            M["for Audi, Škoda, SEAT,"] = "لسيارات أودي وسكودا وسيات";
            M["Volkswagen & CUPRA"] = "وفولكس فاجن وكوبرا";
            M["Book Your Service"] = "احجز خدمتك";
            M["Select Your Brand"] = "اختر ماركتك";
            M["About Czech Service Center"] = "عن المركز التشيكي للصيانة";
            M["The Czech Service Center is your specialized destination for professional maintenance and repair services for Audi, Škoda, SEAT, Volkswagen and CUPRA."] = "المركز التشيكي للصيانة هو وجهتك المتخصصة لخدمات الصيانة والإصلاح الاحترافية لسيارات أودي وسكودا وسيات وفولكس فاجن وكوبرا.";
            M["We use the latest diagnostic technology and genuine parts to keep your car in perfect condition, with the highest standards of quality and care."] = "نستخدم أحدث تقنيات التشخيص وقطع الغيار الأصلية للحفاظ على سيارتك في أفضل حالة، وبأعلى معايير الجودة والعناية.";
            M["Expert Technicians"] = "فنيون خبراء";
            M["Advanced Diagnostics"] = "تشخيص متطور";
            M["Total Trust"] = "ثقة كاملة";
            M["Czech Service Center workshop building"] = "مبنى ورشة المركز التشيكي للصيانة";
            M["Genuine spare parts"] = "قطع غيار أصلية";
            M["for Lasting Performance"] = "لأداء يدوم طويلًا";
            M["We are the official importer of genuine spare parts for Audi, Škoda, SEAT, Volkswagen & CUPRA."] = "نحن المستورد الرسمي لقطع الغيار الأصلية لسيارات أودي وسكودا وسيات وفولكس فاجن وكوبرا.";
            M["Learn More"] = "اعرف المزيد";
            M["Book Your Service Online"] = "احجز خدمتك أونلاين";
            M["Choose your brand, select the service, and book your appointment in just a few clicks."] = "اختر الماركة والخدمة واحجز موعدك بخطوات بسيطة.";
            M["Select Brand"] = "اختر الماركة";
            M["Choose Service"] = "اختر الخدمة";
            M["Pick Date & Time"] = "اختر التاريخ والوقت";
            M["Confirm Booking"] = "تأكيد الحجز";
            M["Book Now"] = "احجز الآن";
            M["Select Branch"] = "اختر الفرع";
            M["Select Service"] = "اختر الخدمة";
            M["Select Time"] = "اختر الوقت";
            M["Branch"] = "الفرع";
            M["Our branches"] = "فروعنا";
            M["Audi, Škoda, SEAT, Volkswagen & CUPRA service"] = "صيانة أودي وسكودا وسيات وفولكس فاجن وكوبرا";
            M["Choose your branch and brand, select the service and pick a time that suits you."] = "اختر الفرع والماركة والخدمة ثم حدد الموعد المناسب لك.";
            M["Thank you! Your booking request was received. We will contact you by phone to confirm it."] = "شكرًا لك! تم استلام طلب الحجز وسنتصل بك هاتفيًا لتأكيده.";
            M["My vehicle"] = "سيارتي";
            M["Not listed / another car"] = "غير موجودة / سيارة أخرى";
            M["Your name"] = "اسمك";
            M["Phone"] = "الهاتف";
            M["Email"] = "البريد الإلكتروني";
            M["Car model / plate"] = "موديل السيارة / رقم اللوحة";
            M["e.g. Octavia 2019, ABC 1234"] = "مثال: أوكتافيا 2019، أ ب ج 1234";
            M["Brand"] = "الماركة";
            M["Service"] = "الخدمة";
            M["Date"] = "التاريخ";
            M["Time"] = "الوقت";
            M["Notes"] = "ملاحظات";
            M["(optional)"] = "(اختياري)";
            M["Describe the problem or anything we should know"] = "اشرح المشكلة أو أي شيء تود أن نعرفه";
            M["Opening hours: {0}. Your booking is confirmed once the center approves it."] = "ساعات العمل: {0}. يتم تأكيد حجزك بعد موافقة المركز.";
            M["No account needed."] = "لا تحتاج إلى حساب.";
            M["if you want to track your bookings."] = "إذا أردت متابعة حجوزاتك.";
            M["{0} to {1}, closed on {2}s"] = "من {0} إلى {1}، مغلق يوم {2}";
            M["Please choose a branch, a brand, a service, a date and a time."] = "من فضلك اختر الفرع والماركة والخدمة والتاريخ والوقت.";
            M["Please choose a date from today onwards."] = "من فضلك اختر تاريخًا من اليوم فصاعدًا.";
            M["The service center is closed on {0}s."] = "المركز مغلق يوم {0}.";
            M["Please choose a valid time."] = "من فضلك اختر وقتًا صحيحًا.";
            M["That time has already passed."] = "هذا الوقت قد مضى بالفعل.";
            M["Sorry, that time was just taken. Please pick another slot."] = "عذرًا، تم حجز هذا الموعد للتو. من فضلك اختر موعدًا آخر.";
            M["That slot is full. Choose another time."] = "هذا الموعد مكتمل. اختر وقتًا آخر.";
            M["Please choose a valid branch."] = "من فضلك اختر فرعًا صحيحًا.";
            M["Booking not found."] = "الحجز غير موجود.";
            M["Please enter your name."] = "من فضلك أدخل اسمك.";
            M["Please enter a valid phone number so we can contact you."] = "من فضلك أدخل رقم هاتف صحيحًا لنتمكن من التواصل معك.";
            M["Please enter a valid email address or leave it empty."] = "من فضلك أدخل بريدًا إلكترونيًا صحيحًا أو اتركه فارغًا.";
            M["This phone number already has 3 upcoming bookings. Please call us to change them."] = "يوجد لهذا الرقم 3 حجوزات قادمة بالفعل. من فضلك اتصل بنا لتعديلها.";
            M["{0} service"] = "خدمة {0}";
            M["Book a {0} service"] = "احجز خدمة {0}";
            M["Browse all {0} parts"] = "تصفح كل قطع {0}";
            M["Services"] = "الخدمات";
            M["About {0} min"] = "حوالي {0} دقيقة";
            M["from {0}"] = "ابتداءً من {0}";
            M["Book this service"] = "احجز هذه الخدمة";
            M["Popular genuine parts"] = "قطع الغيار الأصلية الأكثر طلبًا";
            M["Official spare parts for Audi, Škoda, SEAT, Volkswagen & CUPRA. Search the catalog and reserve what you need."] = "قطع غيار أصلية لسيارات أودي وسكودا وسيات وفولكس فاجن وكوبرا. ابحث في الكتالوج واحجز ما تحتاجه.";
            M["Reserve: {0}"] = "حجز: {0}";
            M["{0} each · {1} in stock"] = "{0} للقطعة · {1} في المخزون";
            M["Quantity"] = "الكمية";
            M["Not specified"] = "غير محدد";
            M["Add to a service booking"] = "إضافة إلى حجز صيانة";
            M["Pick up at the center"] = "الاستلام من المركز";
            M["Confirm reservation"] = "تأكيد حجز القطعة";
            M["Cancel"] = "إلغاء";
            M["Search by name or part number"] = "ابحث بالاسم أو رقم القطعة";
            M["Search parts"] = "ابحث عن القطع";
            M["Search"] = "بحث";
            M["Category"] = "النوع";
            M["Availability"] = "التوفر";
            M["All brands"] = "كل الماركات";
            M["All types"] = "كل الأنواع";
            M["Any availability"] = "أي توفر";
            M["Available"] = "متوفر";
            M["Low stock"] = "مخزون منخفض";
            M["Out of stock"] = "نفد من المخزون";
            M["{0} parts found"] = "تم العثور على {0} قطعة";
            M["1 part found"] = "تم العثور على قطعة واحدة";
            M["No parts match your search. Try another name, brand or type."] = "لا توجد قطع مطابقة لبحثك. جرّب اسمًا أو ماركة أو نوعًا آخر.";
            M["Reserve"] = "احجز";
            M["Previous"] = "السابق";
            M["Next"] = "التالي";
            M["Page {0} of {1}"] = "صفحة {0} من {1}";
            M["Filters"] = "فلاتر";
            M["Brakes"] = "فرامل";
            M["Engine"] = "محرك";
            M["Suspension"] = "مقدمة";
            M["Electrical"] = "كهرباء";
            M["Fluids"] = "سوائل";
            M["Body"] = "هيكل";
            M["Accessories"] = "إكسسوارات";
            M["Please enter a valid quantity (1 to 10)."] = "من فضلك أدخل كمية صحيحة (من 1 إلى 10).";
            M["Sorry, the requested quantity is no longer available."] = "عذرًا، الكمية المطلوبة لم تعد متوفرة.";
            M["Part reserved. You can follow it from My Account."] = "تم حجز القطعة. يمكنك متابعتها من حسابي.";
            M["Contact us"] = "اتصل بنا";
            M["Questions about a service or a part? Send us a message and we will reply by phone or email."] = "لديك سؤال عن خدمة أو قطعة؟ أرسل لنا رسالة وسنرد عليك بالهاتف أو البريد الإلكتروني.";
            M["Visit or call"] = "زورنا أو اتصل بنا";
            M["Open {0}"] = "مواعيد العمل: {0}";
            M["Full name"] = "الاسم بالكامل";
            M["Message"] = "الرسالة";
            M["Send message"] = "إرسال الرسالة";
            M["Please fill in your name, email and message."] = "من فضلك أدخل اسمك وبريدك الإلكتروني ورسالتك.";
            M["Thank you. Your message has been sent and we will get back to you soon."] = "شكرًا لك. تم إرسال رسالتك وسنرد عليك قريبًا.";
            M["Access your vehicles, bookings and service history."] = "ادخل إلى سياراتك وحجوزاتك وسجل الصيانة.";
            M["Password"] = "كلمة المرور";
            M["Keep me signed in on this device"] = "أبقني مسجلًا على هذا الجهاز";
            M["New customer?"] = "عميل جديد؟";
            M["Create an account"] = "أنشئ حسابًا";
            M["Create account"] = "إنشاء حساب";
            M["Create your account"] = "أنشئ حسابك";
            M["Book services, reserve genuine parts and follow your car's service history."] = "احجز خدمات الصيانة، واحجز قطع الغيار الأصلية، وتابع سجل صيانة سيارتك.";
            M["Mobile number"] = "رقم الموبايل";
            M["(at least 8 characters)"] = "(8 أحرف على الأقل)";
            M["Confirm password"] = "تأكيد كلمة المرور";
            M["Already registered?"] = "لديك حساب بالفعل؟";
            M["Email or password is incorrect."] = "البريد الإلكتروني أو كلمة المرور غير صحيحة.";
            M["Please enter your full name."] = "من فضلك أدخل اسمك بالكامل.";
            M["Please enter a valid email address."] = "من فضلك أدخل بريدًا إلكترونيًا صحيحًا.";
            M["Please enter a valid mobile number."] = "من فضلك أدخل رقم موبايل صحيحًا.";
            M["Password must be at least 8 characters."] = "يجب ألا تقل كلمة المرور عن 8 أحرف.";
            M["The two passwords do not match."] = "كلمتا المرور غير متطابقتين.";
            M["An account with this email already exists. Try logging in."] = "يوجد حساب بهذا البريد الإلكتروني بالفعل. جرّب تسجيل الدخول.";
            M["Welcome, {0}. Manage your vehicles, bookings and service history."] = "مرحبًا {0}. أدر سياراتك وحجوزاتك وسجل الصيانة.";
            M["Book a service"] = "احجز خدمة";
            M["My Vehicles"] = "سياراتي";
            M["No vehicles yet. Add your car below so we can keep track of its service history."] = "لا توجد سيارات بعد. أضف سيارتك بالأسفل لنتابع سجل صيانتها.";
            M["VIN {0}"] = "رقم الشاسيه {0}";
            M["Last service {0} at {1}"] = "آخر صيانة بتاريخ {0} عند {1}";
            M["by {0}"] = "بواسطة {0}";
            M["Parts changed: {0}"] = "القطع المستبدلة: {0}";
            M["Cost: {0}"] = "التكلفة: {0}";
            M["Current mileage (km)"] = "عداد الكيلومترات الحالي (كم)";
            M["Update"] = "تحديث";
            M["Remove"] = "إزالة";
            M["Remove this vehicle?"] = "هل تريد إزالة هذه السيارة؟";
            M["Add a vehicle"] = "إضافة سيارة";
            M["Model"] = "الموديل";
            M["e.g. Octavia"] = "مثال: أوكتافيا";
            M["Year"] = "السنة";
            M["Plate number"] = "رقم اللوحة";
            M["Chassis / VIN"] = "رقم الشاسيه / VIN";
            M["Save vehicle"] = "حفظ السيارة";
            M["My Bookings"] = "حجوزاتي";
            M["Status"] = "الحالة";
            M["Progress"] = "التقدم";
            M["No bookings yet."] = "لا توجد حجوزات بعد.";
            M["Cancel this booking?"] = "هل تريد إلغاء هذا الحجز؟";
            M["Maintenance History"] = "سجل الصيانة";
            M["Vehicle"] = "السيارة";
            M["Mileage"] = "عداد الكيلومترات";
            M["Work done"] = "العمل المنفذ";
            M["Parts changed"] = "القطع المستبدلة";
            M["Cost"] = "التكلفة";
            M["Technician"] = "الفني";
            M["Your service history will appear here after your first visit."] = "سيظهر سجل صيانتك هنا بعد زيارتك الأولى.";
            M["My Part Reservations"] = "حجوزات القطع الخاصة بي";
            M["Part"] = "القطعة";
            M["Qty"] = "الكمية";
            M["Total"] = "الإجمالي";
            M["Reserved on"] = "تاريخ الحجز";
            M["No reservations yet."] = "لا توجد حجوزات للقطع بعد.";
            M["Browse genuine parts"] = "تصفح قطع الغيار الأصلية";
            M["Cancel this reservation?"] = "هل تريد إلغاء هذا الحجز للقطعة؟";
            M["Personal Details"] = "البيانات الشخصية";
            M["Save changes"] = "حفظ التغييرات";
            M["No service history yet"] = "لا يوجد سجل صيانة بعد";
            M["Service overdue (due {0})"] = "الصيانة متأخرة (موعدها {0})";
            M["Service due soon ({0})"] = "موعد الصيانة قريب ({0})";
            M["Next service: {0}"] = "الصيانة القادمة: {0}";
            M["{0} or {1}"] = "{0} أو {1}";
            M["Your booking request was received. We will confirm it shortly."] = "تم استلام طلب الحجز وسنؤكده قريبًا.";
            M["Welcome! Add your vehicle below to get started."] = "أهلًا بك! أضف سيارتك بالأسفل للبدء.";
            M["Please enter your name and a valid mobile number."] = "من فضلك أدخل اسمك ورقم موبايل صحيحًا.";
            M["Your details were saved."] = "تم حفظ بياناتك.";
            M["Please fill in brand, model, a valid year and the plate number."] = "من فضلك أدخل الماركة والموديل وسنة صحيحة ورقم اللوحة.";
            M["Mileage must be a positive number."] = "يجب أن تكون قراءة العداد رقمًا موجبًا.";
            M["Enter a valid mileage."] = "أدخل قراءة عداد صحيحة.";
            M["Mileage updated."] = "تم تحديث قراءة العداد.";
            M["Vehicle removed."] = "تم حذف السيارة.";
            M["This vehicle has service history and cannot be removed."] = "لهذه السيارة سجل صيانة ولا يمكن حذفها.";
            M["Booking cancelled."] = "تم إلغاء الحجز.";
            M["This booking can no longer be cancelled."] = "لا يمكن إلغاء هذا الحجز بعد الآن.";
            M["Reservation cancelled."] = "تم إلغاء حجز القطعة.";
            M["Pending"] = "قيد الانتظار";
            M["Approved"] = "تمت الموافقة";
            M["Completed"] = "مكتمل";
            M["Cancelled"] = "ملغي";
            M["Reserved"] = "محجوزة";
            M["Collected"] = "تم الاستلام";
            M["Scheduled"] = "مجدول";
            M["Under Inspection"] = "قيد الفحص";
            M["In Progress"] = "جاري العمل";
            M["Ready For Pickup"] = "جاهزة للاستلام";
            M["Guest"] = "زائر";
            M["Administration"] = "لوحة الإدارة";
            M["Dashboard"] = "لوحة التحكم";
            M["Bookings"] = "الحجوزات";
            M["Parts & Inventory"] = "القطع والمخزون";
            M["Maintenance Records"] = "سجلات الصيانة";
            M["Admin Dashboard"] = "لوحة تحكم الإدارة";
            M["Bookings today"] = "حجوزات اليوم";
            M["Waiting for approval"] = "في انتظار الموافقة";
            M["Cars in the workshop"] = "سيارات داخل الورشة";
            M["Customers"] = "العملاء";
            M["Active parts"] = "القطع النشطة";
            M["Low or out of stock"] = "مخزون منخفض أو نافد";
            M["Upcoming bookings"] = "الحجوزات القادمة";
            M["Customer"] = "العميل";
            M["Car"] = "السيارة";
            M["No upcoming bookings."] = "لا توجد حجوزات قادمة.";
            M["Manage all bookings"] = "إدارة كل الحجوزات";
            M["Latest messages"] = "أحدث الرسائل";
            M["No messages yet."] = "لا توجد رسائل بعد.";
            M["All statuses"] = "كل الحالات";
            M["Filter"] = "تصفية";
            M["Reset"] = "إعادة ضبط";
            M["All branches"] = "كل الفروع";
            M["When"] = "الموعد";
            M["Car / service"] = "السيارة / الخدمة";
            M["Workshop progress"] = "تقدم العمل بالورشة";
            M["Reschedule"] = "إعادة الجدولة";
            M["Approve"] = "موافقة";
            M["Complete"] = "إتمام";
            M["Set"] = "تعيين";
            M["Move"] = "نقل";
            M["Note: {0}"] = "ملاحظة: {0}";
            M["No bookings found."] = "لا توجد حجوزات.";
            M["Booking #{0} approved."] = "تمت الموافقة على الحجز رقم {0}.";
            M["Booking #{0} completed."] = "تم إتمام الحجز رقم {0}.";
            M["Booking #{0} cancelled."] = "تم إلغاء الحجز رقم {0}.";
            M["Booking #{0} moved to {1} {2}."] = "تم نقل الحجز رقم {0} إلى {1} {2}.";
            M["Unknown status."] = "حالة غير معروفة.";
            M["Choose a new date and time."] = "اختر تاريخًا ووقتًا جديدين.";
            M["Workshop status updated. The customer can see it in their account."] = "تم تحديث حالة الورشة. يمكن للعميل رؤيتها في حسابه.";
            M["Edit part {0}"] = "تعديل القطعة {0}";
            M["Add a new part"] = "إضافة قطعة جديدة";
            M["Part number"] = "رقم القطعة";
            M["Name"] = "الاسم";
            M["Name (Arabic)"] = "الاسم بالعربية";
            M["Description (Arabic)"] = "الوصف بالعربية";
            M["Price (EGP)"] = "السعر (جنيه)";
            M["Quantity in stock"] = "الكمية المتوفرة";
            M["Description"] = "الوصف";
            M["Image"] = "الصورة";
            M["(JPG, PNG or WebP, max 2 MB)"] = "(JPG أو PNG أو WebP، بحد أقصى 2 ميجابايت)";
            M["Visible in the catalog"] = "ظاهرة في الكتالوج";
            M["Save part"] = "حفظ القطعة";
            M["Search name or part number"] = "ابحث بالاسم أو رقم القطعة";
            M["Any stock"] = "أي مخزون";
            M["Add part"] = "إضافة قطعة";
            M["{0} parts"] = "{0} قطعة";
            M["Price"] = "السعر";
            M["Stock"] = "المخزون";
            M["Visible"] = "ظاهرة";
            M["Hidden"] = "مخفية";
            M["Edit"] = "تعديل";
            M["Delete"] = "حذف";
            M["Delete this part?"] = "هل تريد حذف هذه القطعة؟";
            M["Part saved."] = "تم حفظ القطعة.";
            M["Part deleted."] = "تم حذف القطعة.";
            M["Stock updated."] = "تم تحديث المخزون.";
            M["Enter a valid quantity."] = "أدخل كمية صحيحة.";
            M["This part has reservations, so it was hidden from the catalog instead of deleted."] = "لهذه القطعة حجوزات، لذلك تم إخفاؤها من الكتالوج بدلًا من حذفها.";
            M["A part with this part number already exists."] = "توجد قطعة بنفس رقم القطعة بالفعل.";
            M["Only JPG, PNG or WebP images are allowed."] = "يُسمح فقط بصور JPG أو PNG أو WebP.";
            M["The image must be smaller than 2 MB."] = "يجب أن يكون حجم الصورة أقل من 2 ميجابايت.";
            M["Please fill in part number, name, brand, category, a valid price and quantity."] = "من فضلك أدخل رقم القطعة والاسم والماركة والنوع وسعرًا وكمية صحيحين.";
            M["Record a completed service"] = "تسجيل صيانة مكتملة";
            M["Select a vehicle"] = "اختر سيارة";
            M["Service date"] = "تاريخ الصيانة";
            M["Mileage at service (km)"] = "قراءة العداد عند الصيانة (كم)";
            M["Cost (EGP)"] = "التكلفة (جنيه)";
            M["Next service date"] = "موعد الصيانة القادمة";
            M["(blank = in 6 months)"] = "(اتركه فارغًا = بعد 6 أشهر)";
            M["Next service mileage"] = "كيلومترات الصيانة القادمة";
            M["(blank = +10,000 km)"] = "(اتركه فارغًا = +10,000 كم)";
            M["e.g. Oil filter, air filter, front brake pads"] = "مثال: فلتر زيت، فلتر هواء، تيل فرامل أمامي";
            M["Save record"] = "حفظ السجل";
            M["Latest records"] = "أحدث السجلات";
            M["Owner"] = "المالك";
            M["Work / parts"] = "العمل / القطع";
            M["Next service"] = "الصيانة القادمة";
            M["No records yet."] = "لا توجد سجلات بعد.";
            M["Please choose a vehicle and fill in the date, mileage and cost."] = "من فضلك اختر سيارة واملأ التاريخ وقراءة العداد والتكلفة.";
            M["Service record saved. The customer's next-service alert was updated."] = "تم حفظ سجل الصيانة وتحديث تنبيه الصيانة القادمة للعميل.";
            M["Pages"] = "الصفحات";
            M["Car model"] = "موديل السيارة";
            return M;
        }
    }

    /// <summary>Response filter: buffers the page and translates it when the site language is Arabic.</summary>
    public class LangFilter : Stream
    {
        readonly Stream _inner;
        readonly Encoding _enc;
        readonly MemoryStream _buf = new MemoryStream();
        bool _closed;

        public LangFilter(Stream inner, Encoding enc) { _inner = inner; _enc = enc ?? Encoding.UTF8; }

        public override bool CanRead { get { return false; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return true; } }
        public override long Length { get { throw new NotSupportedException(); } }
        public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
        public override void SetLength(long value) { throw new NotSupportedException(); }
        public override void Write(byte[] buffer, int offset, int count) { _buf.Write(buffer, offset, count); }

        public override void Close()
        {
            if (!_closed)
            {
                _closed = true;
                byte[] data = _buf.ToArray();
                var ctx = HttpContext.Current;
                bool html = ctx == null || (ctx.Response.ContentType ?? "").StartsWith("text/html", StringComparison.OrdinalIgnoreCase);
                if (html && data.Length > 0) data = _enc.GetBytes(Lang.Translate(_enc.GetString(data)));
                if (data.Length > 0) _inner.Write(data, 0, data.Length);
                _inner.Flush();
            }
            base.Close();
        }
    }
}
