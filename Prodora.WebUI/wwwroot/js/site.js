function PaymentMethodChangeEvent(paymentType) {
    var paymentBox = document.getElementById("payment-box");
    if (paymentType == "credit") {
        paymentBox.style.display = "block"
    }
    else {
        paymentBox.style.display = "none"

    }
}