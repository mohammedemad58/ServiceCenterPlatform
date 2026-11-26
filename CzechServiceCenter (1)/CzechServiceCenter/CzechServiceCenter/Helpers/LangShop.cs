using System;
using System.Collections.Generic;

namespace CzechServiceCenter
{
    /// <summary>Arabic texts of the online shop pages (cart, checkout, orders, service schedule). Existing translations are never replaced.</summary>
    static class LangShop
    {
        static void Add(Dictionary<string, string> M, string en, string ar)
        {
            if (!M.ContainsKey(en)) M[en] = ar;
        }

        internal static void Fill(Dictionary<string, string> M)
        {
            Add(M, "Service Schedule", "جدول الصيانة");
            Add(M, "Cart", "السلة");
            Add(M, "Track your order", "تتبع طلبك");
            Add(M, "Shop Genuine Parts", "تسوق قطع الغيار الأصلية");
            Add(M, "Official spare parts for Audi, Škoda, SEAT, Volkswagen & CUPRA. Choose your car, add parts to the cart and pay online. No account needed.", "قطع غيار أصلية لسيارات أودي وسكودا وسيات وفولكس فاجن وكوبرا. اختر سيارتك وأضف القطع إلى السلة وادفع أونلاين. بدون الحاجة إلى حساب.");
            Add(M, "View cart and checkout", "عرض السلة وإتمام الشراء");
            Add(M, "Find parts for your car", "ابحث عن القطع المناسبة لسيارتك");
            Add(M, "Brand", "الماركة");
            Add(M, "Model", "الموديل");
            Add(M, "Year", "سنة الصنع");
            Add(M, "Engine size", "سعة المحرك");
            Add(M, "All models", "كل الموديلات");
            Add(M, "Any year", "أي سنة");
            Add(M, "Any engine", "أي محرك");
            Add(M, "{0} cc", "{0} سي سي");
            Add(M, "Showing only the parts needed for the chosen service.", "يتم عرض القطع المطلوبة للصيانة المختارة فقط.");
            Add(M, "Added to your cart.", "تمت إضافة القطعة إلى السلة.");
            Add(M, "Please enter a valid quantity (1 to 20).", "من فضلك أدخل كمية صحيحة (من 1 إلى 20).");
            Add(M, "Sorry, this part is out of stock.", "عذرًا، هذه القطعة غير متوفرة حاليًا.");
            Add(M, "Add to cart", "أضف إلى السلة");
            Add(M, "Reset", "إعادة ضبط");
            Add(M, "Search", "بحث");
            Add(M, "Your Cart", "سلة المشتريات");
            Add(M, "Review your parts, then pay online by Visa or InstaPay. No account needed.", "راجع القطع ثم ادفع أونلاين بالفيزا أو إنستاباي. بدون الحاجة إلى حساب.");
            Add(M, "Your cart is empty.", "السلة فارغة.");
            Add(M, "Browse genuine parts", "تصفح قطع الغيار الأصلية");
            Add(M, "Part", "القطعة");
            Add(M, "Price", "السعر");
            Add(M, "Quantity", "الكمية");
            Add(M, "Total", "الإجمالي");
            Add(M, "Remove", "حذف");
            Add(M, "Update cart", "تحديث السلة");
            Add(M, "Continue shopping", "متابعة التسوق");
            Add(M, "Subtotal", "المجموع الفرعي");
            Add(M, "Proceed to checkout", "متابعة إلى الدفع");
            Add(M, "Some items are no longer available in the quantity you chose. Update the quantities to continue.", "بعض القطع لم تعد متوفرة بالكمية التي اخترتها. عدّل الكميات للمتابعة.");
            Add(M, "Some items are no longer available in the quantity you chose. Please update your cart.", "بعض القطع لم تعد متوفرة بالكمية التي اخترتها. من فضلك عدّل السلة.");
            Add(M, "Only {0} left in stock", "المتبقي في المخزون {0} فقط");
            Add(M, "Cart updated.", "تم تحديث السلة.");
            Add(M, "Item removed.", "تم حذف القطعة.");
            Add(M, "Checkout", "إتمام الشراء");
            Add(M, "No account needed. Your parts are reserved for you while you pay.", "بدون الحاجة إلى حساب. يتم حجز القطع لك أثناء الدفع.");
            Add(M, "Your details", "بياناتك");
            Add(M, "Full name", "الاسم بالكامل");
            Add(M, "Phone", "رقم الهاتف");
            Add(M, "Email", "البريد الإلكتروني");
            Add(M, "(optional, for your receipt)", "(اختياري، لإرسال الإيصال)");
            Add(M, "Receive your parts", "استلام القطع");
            Add(M, "Pick up at a branch", "الاستلام من أحد الفروع");
            Add(M, "Delivery", "التوصيل");
            Add(M, "Branch", "الفرع");
            Add(M, "Select a branch", "اختر الفرع");
            Add(M, "City", "المدينة");
            Add(M, "Full address", "العنوان بالتفصيل");
            Add(M, "Delivery fee: {0}", "رسوم التوصيل: {0}");
            Add(M, "Payment", "الدفع");
            Add(M, "Visa / Mastercard (card)", "فيزا / ماستركارد (بطاقة)");
            Add(M, "InstaPay transfer", "تحويل إنستاباي");
            Add(M, "You will be taken to the secure payment page of our payment provider. Your card details are never stored on this site.", "سيتم تحويلك إلى صفحة الدفع الآمنة الخاصة بمزوّد الدفع. لا يتم حفظ بيانات بطاقتك على هذا الموقع.");
            Add(M, "After you place the order you will see our InstaPay address. Transfer the total, then send us the transaction reference. We confirm it and prepare your order.", "بعد تأكيد الطلب سيظهر لك عنوان إنستاباي الخاص بنا. حوّل المبلغ الإجمالي ثم أرسل لنا رقم العملية. سنؤكد الدفع ونجهّز طلبك.");
            Add(M, "Place order and pay", "تأكيد الطلب والدفع");
            Add(M, "Order summary", "ملخص الطلب");
            Add(M, "Prices are in Egyptian pounds.", "الأسعار بالجنيه المصري.");
            Add(M, "Edit cart", "تعديل السلة");
            Add(M, "Please enter your full name.", "من فضلك أدخل اسمك بالكامل.");
            Add(M, "Please enter a valid phone number.", "من فضلك أدخل رقم هاتف صحيح.");
            Add(M, "Please enter a valid email address or leave it empty.", "من فضلك أدخل بريدًا إلكترونيًا صحيحًا أو اتركه فارغًا.");
            Add(M, "Please choose pickup or delivery.", "من فضلك اختر الاستلام أو التوصيل.");
            Add(M, "Please choose how you want to pay.", "من فضلك اختر طريقة الدفع.");
            Add(M, "Please choose the branch where you will pick up.", "من فضلك اختر الفرع الذي ستستلم منه.");
            Add(M, "Please enter your city and full delivery address.", "من فضلك أدخل المدينة وعنوان التوصيل بالتفصيل.");
            Add(M, "Sorry, \"{0}\" is no longer available in the quantity you asked for. Please update your cart.", "عذرًا، \"{0}\" لم تعد متوفرة بالكمية المطلوبة. من فضلك عدّل السلة.");
            Add(M, "Track your order", "تتبع طلبك");
            Add(M, "Enter your order number and the phone number you used at checkout.", "أدخل رقم الطلب ورقم الهاتف الذي استخدمته عند الشراء.");
            Add(M, "Order number", "رقم الطلب");
            Add(M, "Find my order", "ابحث عن طلبي");
            Add(M, "Order #{0}", "الطلب رقم {0}");
            Add(M, "Pending", "قيد الانتظار");
            Add(M, "Awaiting Review", "بانتظار المراجعة");
            Add(M, "Paid", "تم الدفع");
            Add(M, "Failed", "فشل");
            Add(M, "Cancelled", "ملغي");
            Add(M, "New", "جديد");
            Add(M, "Processing", "قيد التجهيز");
            Add(M, "Ready", "جاهز");
            Add(M, "Completed", "مكتمل");
            Add(M, "This order was cancelled", "تم إلغاء هذا الطلب");
            Add(M, "The parts were released. If you already paid, please contact us with your order number.", "تم تحرير القطع. إذا كنت قد دفعت بالفعل فتواصل معنا مع ذكر رقم الطلب.");
            Add(M, "Browse parts", "تصفح القطع");
            Add(M, "Payment received. Thank you!", "تم استلام الدفع. شكرًا لك!");
            Add(M, "We will prepare your parts for pickup at {0}. We will call you on {1} when they are ready.", "سنجهّز قطعك للاستلام من {0}. سنتصل بك على {1} عندما تكون جاهزة.");
            Add(M, "We will prepare your parts and arrange delivery. We will call you on {0}.", "سنجهّز قطعك ونرتّب التوصيل. سنتصل بك على {0}.");
            Add(M, "Confirming your payment...", "جارٍ تأكيد الدفع...");
            Add(M, "This page refreshes by itself. It usually takes a few seconds.", "تُحدَّث هذه الصفحة تلقائيًا. يستغرق الأمر عادةً بضع ثوانٍ.");
            Add(M, "Pay by card", "الدفع بالبطاقة");
            Add(M, "Your parts are held for {0} minutes. Complete the payment to confirm your order.", "القطع محجوزة لك لمدة {0} دقيقة. أكمل الدفع لتأكيد طلبك.");
            Add(M, "Pay now by card", "ادفع الآن بالبطاقة");
            Add(M, "We are checking your transfer", "نراجع التحويل الخاص بك");
            Add(M, "Your reference: {0}. We will confirm your payment as soon as we see the transfer, then prepare your order.", "رقم العملية الخاص بك: {0}. سنؤكد الدفع بمجرد ظهور التحويل ثم نجهّز طلبك.");
            Add(M, "Pay with InstaPay", "الدفع عبر إنستاباي");
            Add(M, "Open your InstaPay app and send {0} to:", "افتح تطبيق إنستاباي وحوّل {0} إلى:");
            Add(M, "Copy the transaction reference number from the InstaPay confirmation and enter it here.", "انسخ رقم العملية من تأكيد إنستاباي وأدخله هنا.");
            Add(M, "Transaction reference", "رقم العملية");
            Add(M, "I have paid: send reference", "لقد دفعت: أرسل رقم العملية");
            Add(M, "Your parts are held for {0} hours. After that the order is cancelled if no reference was sent.", "القطع محجوزة لك لمدة {0} ساعة. بعد ذلك يُلغى الطلب إذا لم يتم إرسال رقم العملية.");
            Add(M, "Pickup at {0}", "الاستلام من {0}");
            Add(M, "Ordered {0}", "تاريخ الطلب {0}");
            Add(M, "Thank you. We received your transfer reference and will confirm your payment shortly.", "شكرًا لك. استلمنا رقم العملية وسنؤكد الدفع قريبًا.");
            Add(M, "The card payment was not completed. You can try again below, or pay by InstaPay.", "لم تكتمل عملية الدفع بالبطاقة. يمكنك المحاولة مرة أخرى أدناه أو الدفع عبر إنستاباي.");
            Add(M, "We could not find this order.", "لم نعثر على هذا الطلب.");
            Add(M, "We could not find an order with these details.", "لم نعثر على طلب بهذه البيانات.");
            Add(M, "Please enter the InstaPay transaction reference number.", "من فضلك أدخل رقم عملية إنستاباي.");
            Add(M, "This order can no longer accept a payment reference. Please contact us.", "لا يمكن إضافة رقم عملية لهذا الطلب. من فضلك تواصل معنا.");
            Add(M, "This order cannot be paid any more.", "لا يمكن دفع هذا الطلب بعد الآن.");
            Add(M, "The card payment service is not available right now. Please try again in a moment or pay by InstaPay.", "خدمة الدفع بالبطاقة غير متاحة الآن. حاول مرة أخرى بعد قليل أو ادفع عبر إنستاباي.");
            Add(M, "Test payment", "دفع تجريبي");
            Add(M, "Test mode: no money is charged and no card is needed.", "وضع تجريبي: لا يتم خصم أي مبلغ ولا حاجة لبطاقة.");
            Add(M, "Simulate successful payment", "محاكاة دفع ناجح");
            Add(M, "Simulate failed payment", "محاكاة دفع فاشل");
            Add(M, "Service schedule", "جدول الصيانة");
            Add(M, "What your car needs at every mileage, and the parts to use. Pick your brand and enter your mileage.", "ما تحتاجه سيارتك عند كل عدد من الكيلومترات، والقطع المستخدمة. اختر الماركة وأدخل عداد سيارتك.");
            Add(M, "My car has driven (km)", "الكيلومترات التي قطعتها سيارتي");
            Add(M, "Show my next service", "اعرض صيانتي القادمة");
            Add(M, "At {0} your next service is the {1} service. It is highlighted below.", "عند {0} تكون صيانتك القادمة هي صيانة {1}. تم تمييزها بالأسفل.");
            Add(M, "{0} service by mileage", "صيانة {0} حسب الكيلومترات");
            Add(M, "Every {0}", "كل {0}");
            Add(M, "What we do", "ما يتم عمله");
            Add(M, "Parts you need", "القطع المطلوبة");
            Add(M, "Shop these parts for {0}", "اشترِ هذه القطع لسيارات {0}");
            Add(M, "After 100,000 km the schedule starts again from the 10,000 km service. Always follow your owner's manual for your exact engine and gearbox.", "بعد 100,000 كم يبدأ الجدول من جديد من صيانة 10,000 كم. اتبع دائمًا دليل المالك لمحركك وناقل الحركة الخاص بك.");
            Add(M, "What your car needs at every mileage, and which parts to use.", "ما تحتاجه سيارتك عند كل عدد من الكيلومترات والقطع المستخدمة.");
            Add(M, "See the full schedule", "عرض الجدول الكامل");
        }
    }
}
