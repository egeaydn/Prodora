document.addEventListener('DOMContentLoaded', function () {
    // Add loading state to delete buttons
    const deleteButtons = document.querySelectorAll('form[asp-action="DeleteFromBasket"] button');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function () {
            this.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
            this.disabled = true;
        });
    });

    // Add hover effects to table rows
    const tableRows = document.querySelectorAll('.table tbody tr');
    tableRows.forEach(row => {
        row.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-2px)';
        });

        row.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    });
});