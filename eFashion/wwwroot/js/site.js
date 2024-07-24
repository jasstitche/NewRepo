// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function AddCart(sampleId) {
    debugger;
    $.ajax({
        type: 'Post',
        dataType: 'Json',
        url: '/Cart/Add',
        data: {
            sampleId: sampleId
            //email = email
        },
        success: function (result) {
            debugger
            console.log("AJAX success:", result);

            if (!result.isError) {
                debugger;
                $('#cartCountForAddedCart').text(result.cartCount);
            }
            else {
                errorAlert(result.msg)
            }
        },
        error: function (ex) {
            errorAlert("Error occured try again");
        }
    })
}


function viewCart(email) {
    debugger;
    $.ajax({
        type: 'GET',
        //dataType: 'json',
        url: '/Cart/_CartListPartial',
        data: {
            email: email,
        },
        success: function (result) {
            debugger;
            if (!result.isError) {
                debugger;
                $('#cartContent').html(result);
                $('#cartDetails').modal('show');
            } else {
                errorAlert('Failed to load cart items.');
            }
        },
        error: function (ex) {
            errorAlert("Error occured try again");
        }
    })
}

function deleteCart(id) {
    debugger;
    $.ajax({
        type: 'Post',
        //dataType: 'json',
        url: '/Cart/DeleteCart',
        data: {
            id: id,
        },
        success: function (result) {
            debugger;
            if (!result.isError) {
                viewCart(result.appUser.email)
            } else {
                errorAlert('Failed to delete cart items.');
            }
        },
        error: function (ex) {
            errorAlert("Error occured try again");
        }
    })
}

function updateQuantity(id, change) {
    debugger;
    $.ajax({
        type: 'Post',
        //dataType: 'json',
        url: '/Cart/UpdateQuantity',
        data: {
            id: id,
            change: change
        },
        success: function (result) {
            debugger
            if (!result.isError) {
                var row = $('[data-item-id = "'+ id + '"]');
                var quantitySpan = row.find('.quantity');
                quantitySpan.text(result.quantity);
                var subTotalspan = row.find('.subTotal');
                subTotalspan.text(result.subTotal);
                var decrementspan = row.find('.decrement');
                if (result.quantity == 1)
                {
                   decrementspan.attr("disabled", 'disabled');

                }
            } else {
                decrementspan.removeAttr('disabled');
            }
            viewCart(result.appUser.email)
        },

        error: function (ex) {
            errorAlert("Error occured try again");
        }
    })
}


function checkout() {
    var paymentTypeId = $('#PaymentTypeId').val();

    debugger
    $.ajax({
        type: 'GET',
        url: '/Cart/GetTotalAmount',
        data: {
            paymentTypeId: paymentTypeId,
        },
        success: function (result) {
            debugger
            if (!result.isError) {
                $('#totalAmount').text(result.totalAmount);
                $('#totalAmountToBePay').text(result.totalAmountToPay);
                $('#pickUpAddress').text(result.getCompanySettings.address);
                $('#companyPhoneNumber').text(result.getCompanySettings.phoneNumber);
                $('#companyAccountNumber').text(result.getCompanySettings.accountNumber);
                $('#companyAccountName').text(result.getCompanySettings.accountName);
                $('#companyBankName').text(result.getCompanySettings.bankName);
                $('#companyAddress').text(result.getCompanySettings.companyAddress);
                $('#deliveryAddress').text(result.getCompanySettings.deliveryAddress);
                $('#deliveryFee').text(result.getCompanySettings.deliveryFee);
                //$('#selectedPaymentType').text(result.paymentTypeId);
                var selectedPaymentType = $('#PaymentTypeId').val();
                $('#paymentTypeId').val(paymentTypeId);
                $('#totalAmountToPay').val(result.totalAmountToPay);


                if (selectedPaymentType == '1') {

                    $('#transferModal').modal('show');
                } else if (selectedPaymentType == '2') {
                    $('#cashModal').modal('show');
                }
            } else {
                errorAlert("Error occurred while fetching the total amount. Please try again.");
            }
        },
        error: function (ex) {
            errorAlert("Error occurred. Please try again.");
        }
    });
}

function makePayment() {
    debugger
    $.ajax({
        type: 'GET',
        url: '/Orders/Orderspage',
        success: function (result) {
            debugger
            if (!result.isError) {
                $('#totalAmount').text(result.totalAmount);
                $('#totalQuantity').text(result.totalQuantity);
                //$('#quantity').text(result.quantity);
                //$('#designName').text(result.designName);
                $('#quantity').empty();
                result.quantity.forEach(function (qty) {
                    $('#quantity').append('<div>' + qty + '</div>');
                });

                $('#designName').empty();
                result.designName.forEach(function (name) {
                    $('#designName').append('<div>' + name + '</div>');
                });

            } else {
                errorAlert("Error occurred while fetching the total amount. Please try again.");
            }
        },
        error: function (ex) {
            errorAlert("Error occurred. Please try again.");
        }
    });
}


function makePayment() {
    $('#transferModal').modal('hide');
    $('#checkoutModal').modal('show');
}

function displayFileDetails(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $('#filePreview').html('<img src="' + e.target.result + '" class="img-fluid" style="max-width: 100px; max-height: 100px;">');
            $('#filePreview').removeClass('d-none');
        };

        reader.readAsDataURL(input.files[0]); // Read the file as data URL
    }
}
//function decreaseQuantity(id) {

//    $.ajax({

//    };

//}

//<script>
//    function AddCart(sampleId) {
//        $.ajax({
//            url: '@Url.Action("AddToCart", "Cart")',
//            type: 'POST',
//            data: { id: sampleId },
//            success: function (data) {
//                $('#modalBodyContent').html(data);
//                $('#myModal').modal('show');
//            }
//        });
//        }
//</script>

//function removeItem(button) {
//    debugger
//    // Get the row of the button
//    var row = button.parentNode.parentNode;
//    // Remove the row from the table
//    row.parentNode.removeChild(row);
//}


function closeModal(modalId) {
    $('#' + modalId).modal('hide');
}

function paymentApproval(paymentId) {
    debugger;
    $.ajax({
        type: 'Post',
        dataType: 'Json',
        url: '/Admin/ApprovePayment',
        data: {
            paymentId: paymentId
        },
        success: function (result) {
            if (!result.isError) {
                debugger;
                successAlert(result.msg);
            }
            else {
                errorAlert(result.msg)
            }
        },
        error: function (ex) {
            errorAlert("Error occured try again");
        }
    })
}

function declinePayment(paymentId) {
    debugger;
    $.ajax({
        type: 'Post',
        dataType: 'Json',
        url: '/Admin/DeclinePayment',
        data: {
            paymentId: paymentId
        },
        success: function (result) {
            if (!result.isError) {
                debugger;
                successAlert(result.msg);
            }
            else {
                errorAlert(result.msg)
            }
        },
        error: function (ex) {
            errorAlert("Error occured try again");
        }
    })
}

