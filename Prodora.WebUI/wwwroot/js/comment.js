document.addEventListener('DOMContentLoaded', function () {
    const commentInput = document.getElementById('new_comment_text');
    const submitBtn = document.getElementById('submitCommentBtn');

    // Character counter
    if (commentInput) {
        commentInput.addEventListener('input', function () {
            const remaining = 500 - this.value.length;
            if (remaining < 50) {
                this.style.borderColor = remaining < 0 ? 'var(--danger-color)' : 'var(--warning-color)';
            } else {
                this.style.borderColor = 'var(--border-color)';
            }
        });
    }

    // Enhanced button states
    const deleteButtons = document.querySelectorAll('.delete-comment-btn');
    deleteButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            if (confirm('Bu yorumu silmek istediğinizden emin misiniz?')) {
                this.classList.add('loading-state');
                this.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
            }
        });
    });
});

function doComment(action, commentId) {
    var text = "";
    var rating = 0;
    const submitBtn = document.getElementById('submitCommentBtn');

    if (action === "new_clicked") {
        text = $("#new_comment_text").val().trim();
        rating = parseInt($("#new_comment_rating").val()) || 0;

        if (!text) {
            alert("Lütfen bir yorum girin.");
            $("#new_comment_text").focus();
            return;
        }

        if (text.length > 500) {
            alert("Yorum 500 karakterden uzun olamaz.");
            return;
        }

        // Show loading state
        if (submitBtn) {
            submitBtn.classList.add('loading-state');
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Gönderiliyor...';
            submitBtn.disabled = true;
        }
    }

    $.ajax({
        url: "/Comment/" + (action === "new_clicked" ? "Create" : "Delete"),
        type: "POST",
        data: {
            text: text,
            productId: productId,
            id: commentId,
            raiting: rating
        },
        success: function (response) {
            if (response.result) {
                if (action === "new_clicked") {
                    // Clear form
                    $("#new_comment_text").val('');
                    $("#new_comment_rating").val('3');
                }
                location.reload();
            } else {
                var msg = response.message || "İşlem başarısız oldu. Lütfen tekrar deneyin.";
                if (response.errors && response.errors.length > 0) {
                    msg += "\n" + response.errors.join("\n");
                }
                alert(msg);
            }
        },
        error: function (xhr) {
            alert("Sunucu hatası!\n" + (xhr.responseText || "Lütfen daha sonra tekrar deneyin."));
        },
        complete: function () {
            // Reset button state
            if (submitBtn && action === "new_clicked") {
                submitBtn.classList.remove('loading-state');
                submitBtn.innerHTML = '<i class="fas fa-paper-plane"></i> Yorum Yap';
                submitBtn.disabled = false;
            }
        }
    });
}