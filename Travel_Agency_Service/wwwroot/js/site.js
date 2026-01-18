document.addEventListener('DOMContentLoaded', () => {

    const passwordInput = document.getElementById("password");
    const hint = document.getElementById("passwordHint");

    if (passwordInput && hint) {
        passwordInput.addEventListener("input", () => {
            const value = passwordInput.value;
            const hasLetter = /[a-zA-Z]/.test(value);
            const hasNumber = /[0-9]/.test(value);

            if (value.length > 0 && (!hasLetter || !hasNumber || value.length < 6)) {
                hint.style.display = "block";
            } else {
                hint.style.display = "none";
            }
        });
    }

});

function addServiceReview() {

    const rating = document.getElementById("serviceRating").value;
    const comment = document.getElementById("serviceComment").value;
    const errorDiv = document.getElementById("serviceReviewError");

    errorDiv.innerText = "";

    if (!rating || !comment.trim()) {
        errorDiv.innerText = "Please fill rating and comment";
        return;
    }

    fetch("/ServiceReviews/Add", {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded"
        },
        body: `rating=${rating}&comment=${encodeURIComponent(comment)}`
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                const modal = bootstrap.Modal.getInstance(
                    document.getElementById("addServiceReviewModal")
                );
                modal.hide();
                location.reload();
            } else {
                errorDiv.innerText = data.message || "Failed to add review";
            }
        })
        .catch(() => {
            errorDiv.innerText = "Server error";
        });
}

function addTripReview(tripId) {

    const rating = document.getElementById(`tripRating-${tripId}`).value;
    const comment = document.getElementById(`tripComment-${tripId}`).value;
    const errorDiv = document.getElementById(`tripReviewError-${tripId}`);

    errorDiv.innerText = "";

    if (!rating || !comment.trim()) {
        errorDiv.innerText = "Please fill rating and comment";
        return;
    }

    fetch("/TripReviews/Add", {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded"
        },
        body:
            `tripId=${tripId}&rating=${rating}&comment=${encodeURIComponent(comment)}`
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                bootstrap.Modal
                    .getInstance(
                        document.getElementById(`addTripReviewModal-${tripId}`)
                    ).hide();
                location.reload();
            } else {
                errorDiv.innerText = data.message;
            }
        });
}

function deleteTripReview(reviewId, tripId) {
    if (!confirm('Delete this review?')) return;

    fetch('/TripReviews/Delete', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken':
                document.getElementById('antiForgeryToken').value
        },
        body: 'reviewId=' + reviewId
    })
        .then(res => res.json())
        .then(result => {
            if (result.success) {
                loadTripReviews(tripId); 
            } else {
                alert(result.message);
            }
        });
}


function deleteServiceReview(reviewId) {
    if (!confirm('Delete this review?')) return;

    fetch('/ServiceReviews/Delete', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken':
                document.getElementById('antiForgeryToken').value
        },
        body: 'reviewId=' + reviewId
    })
        .then(res => res.json())
        .then(result => {
            if (result.success) {
                location.reload();
            } else {
                alert(result.message);
            }
        });
}



document.addEventListener("DOMContentLoaded", function () {

    document.querySelectorAll(".star-rating").forEach(container => {

        const stars = container.querySelectorAll("span");
        const hiddenInput = container.querySelector("input[type='hidden']");
        let selectedRating = 0;

        stars.forEach(star => {

            // hover – רק תצוגה
            star.addEventListener("mouseenter", () => {
                highlight(parseInt(star.dataset.value));
            });

            // click – קובע דירוג
            star.addEventListener("click", () => {
                selectedRating = parseInt(star.dataset.value);
                hiddenInput.value = selectedRating;
                highlight(selectedRating);
            });
        });

        // יציאה מהאזור – חוזר למה שנבחר
        container.addEventListener("mouseleave", () => {
            highlight(selectedRating);
        });

        function highlight(value) {
            stars.forEach(s => {
                s.classList.toggle(
                    "filled",
                    parseInt(s.dataset.value) <= value
                );
            });
        }
    });

});
