window.Notifications = {
    showSuccess: function (message) {
        Swal.fire({
            icon: 'success',
            title: 'Success',
            text: message,
            timer: 2000,
            showConfirmButton: false
        });
    },
    showError: function (message) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: message
        });
    },
    showConfirm: async function (message) {
        const result = await Swal.fire({
            title: 'Are you sure?',
            text: message,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, proceed!'
        });
        return result.isConfirmed;
    }
};
