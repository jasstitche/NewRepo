// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//function registration() {
//    debugger;
//    var data = {
//        FirstName: $("#FirstName").val(),
//        LastName: $("#LastName").val(),
//        PhoneNumber: $("#PhoneNumber").val(),
//        Gender: $("#Gender").val(),
//        Address: $("#Address").val(),
//        Email: $("#Email").val(),
//        Password: $("#Password").val(),
//        ConfirmPassword: $("#ConfirmPassword").val()
//    };

//    // Validate inputs before making AJAX request
//    let isValid = true;
//    $('.text-danger').text(''); // Clear previous error messages

//    if (!data.FirstName) {
//        $('#FirstNameError').text('First Name is required');
//        isValid = false;
//    }
//    if (!data.LastName) {
//        $('#LastNameError').text('Last Name is required');
//        isValid = false;
//    }
//    if (!data.PhoneNumber) {
//        $('#PhoneNumberError').text('Phone Number is required');
//        isValid = false;
//    }
//    if (!data.Gender) {
//        $('#GenderError').text('Gender is required');
//        isValid = false;
//    }
//    if (!data.Address) {
//        $('#AddressError').text('Address is required');
//        isValid = false;
//    }
//    if (!data.Email) {
//        $('#EmailError').text('Email is required');
//        isValid = false;
//    }
//    if (!data.Password) {
//        $('#PasswordError').text('Password is required');
//        isValid = false;
//    }
//    if (data.Password !== data.ConfirmPassword) {
//        $('#ConfirmPasswordError').text('Passwords do not match');
//        isValid = false;
//    }

//    if (!isValid) {
//        return;
//    }

//    $.ajax({
//        type: 'POST',
//        url: '/Account/Register',
//        data: JSON.stringify(data),
//        contentType: 'application/json; charset=utf-8',
//        dataType: 'json',
//        success: function (result) {
//            console.log("AJAX success:", result);
//            if (result.success) {
//                alert('Registration successful!');
//                window.location.href = '/Account/Login';
//            } else {
//                Object.keys(result.errors).forEach(function (key) {
//                    $('#' + key + 'Error').text(result.errors[key]);
//                });
//            }
//        },
//        error: function (ex) {
//            $('#validationSummary').text("Error occurred. Please try again.");
//        }
//    });
//}

function registration() {
    debugger;
    var data = {
        FirstName: $("#FirstName").val(),
        LastName: $("#LastName").val(),
        PhoneNumber: $("#PhoneNumber").val(),
        Gender: $("#Gender").val(),
        Address: $("#Address").val(),
        Email: $("#Email").val(),
        Password: $("#Password").val(),
        ConfirmPassword: $("#ConfirmPassword").val()
    };

    // Validate inputs before making AJAX request
    //let isValid = true;
    //$('.text-danger').text(''); // Clear previous error messages

    if (data.FirstName == "") {
        errorAlert("Firstname is required");
        return;
    }

    if (data.LastName == "") {
        errorAlert("LastName is required");
        return;
    }
    if (data.PhoneNumber == "") {
        errorAlert("PhoneNumber is required");
        return;
    }
    if (data.Gender == "") {
        errorAlert("Gender is required");
        return;
    }
    if (data.Address == "") {
        errorAlert("Address is required");
        return;
    }
    if (data.Email == "") {
        errorAlert("Email is required");
        return;
    }
    if (data.Password == "") {
        errorAlert("Password is required");
        return;
    }
    if (data.Password !== data.ConfirmPassword) {
        errorAlert("ConfirmPassword does not match password");
        return;
    }

   
    $.ajax({
        type: 'POST',
        url: '/Account/Register',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (result) {
            debugger;
            
            if (result.isError) {
                errorAlert(result.msg)
               // $('#validationSummary').text(result.msg);
            } else {
                var url = '/Account/Login';
                successAlertWithRedirect(result.msg, url);
            }
        },
        error: function (ex) {
            $('#validationSummary').text("Error occurred. Please try again.");
        }
    });
}

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
                successAlert(result.msg);
            }
            else {
                var url = '/Shop/Shop';
                errorAlert(result.msg, url);
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
            debugger
            newErrorAlert("Error occured try again");
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
                //viewCart(result.appUser.email);
                $('[data-item-id="' + id + '"]').closest('tr').remove();

                $('#cartCountForAddedCart').text(result.cartCount); // Update the cart count directly
                successAlert(result.msg);
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
        type: 'POST',
        url: '/Cart/UpdateQuantity',
        data: {
            id: id,
            change: change
        },
        success: function (result) {
            debugger;
            var row = $('[data-item-id="' + id + '"]');
            var decrementspan = row.find('.decrement');

            if (!result.isError) {
                var quantitySpan = row.find('.quantity');
                quantitySpan.text(result.quantity);
                var subTotalspan = row.find('.subTotal');
                subTotalspan.text(result.subTotal);

                if (result.quantity == 1) {
                    decrementspan.attr("disabled", 'disabled');
                } else {
                    decrementspan.removeAttr('disabled');
                }

                viewCart(result.appUser.email);
            } else {
                decrementspan.removeAttr('disabled');
                var url = '/Shop/Shop';
                errorAlert( result.msg, url);
            }
        },
        error: function (ex) {
            errorAlert("Error occurred, try again");
        }
    });
}



function checkout() {
    var paymentTypeId = $('#PaymentTypeId').val();

    debugger;
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
                if (result.paymentTypeId == '1') {
                    $('#deliveryFee').text(result.getCompanySettings.deliveryFee);

                }
                //$('#selectedPaymentType').text(result.paymentTypeId);
                //var selectedPaymentType = $('#PaymentTypeId').val();
                $('#transferPaymentTypeId').val(result.paymentTypeId);
                $('#cashPaymentTypeId').val(result.paymentTypeId);
                $('#totalAmountToPay').val(result.totalAmountToPay);


                if (result.paymentTypeId == '1') {

                    $('#transferModal').modal('show');
                } else if (result.paymentTypeId == '2') {
                    $('#cashModal').modal('show');
                }
            } else {
                newErrorAlert("Error occurred while fetching the total amount. Please try again.");
            }
        },
        error: function (ex) {
            newErrorAlert("Error occurred. Please try again.");
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
                newErrorAlert("Error occurred while fetching the total amount. Please try again.");
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
                newSuccessAlert(result.msg);
                //location.reload();
            }
            else {
                newErrorAlert(result.msg)
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
                newSuccessAlert(result.msg);
                //location.reload();
            }
            else {
                newErrorAlert(result.msg)
            }
        },
        error: function (ex) {
            errorAlert("Error occured try again");
        }
    })
}

function shipped(paymentId) {
    debugger;
    $.ajax({
        type: 'Post',
        dataType: 'Json',
        url: '/Orders/Shipped',
        data: {
            paymentId: paymentId
        },
        success: function (result) {
            if (!result.isError) {
                debugger;
                newSuccessAlert(result.msg);
                //location.reload();
            }
            else {
                newErrorAlert(result.msg)
            }
        },
        error: function (ex) {
            errorAlert("Error occured try again");
        }
    })
}
function Received(paymentId) {
    debugger;
    $.ajax({
        type: 'Post',
        dataType: 'Json',
        url: '/Admin/Received',
        data: {
            paymentId: paymentId
        },
        success: function (result) {
            if (!result.isError) {
                debugger;
                newSuccessAlert(result.msg);
                //location.reload();
            }
            else {
                newErrorAlert(result.msg)
            }
        },
        error: function (ex) {
            errorAlert("Error occured try again");
        }
    })
}

function deleteSample(id) {
    debugger;
    $.ajax({
        type: 'Post',
        //dataType: 'json',
        url: '/Shop/DeleteSampleById',
        data: {
            id: id,
        },
        success: function (result) {
            debugger;
            if (!result.isError) {
                newSuccessAlert(result.msg);
                //location.reload();
            }
            else {
                newErrorAlert('Failed to delete Sample items.');
            }
        },
        error: function (ex) {
            errorAlert("Error occured try again");
        }
    })
}