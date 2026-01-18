document.addEventListener("DOMContentLoaded", function () {
    const emailInput = document.getElementById("emailInput");
    if (!emailInput) return;

    emailInput.addEventListener("blur", function () {
        const value = emailInput.value.trim();

        // אם אין @ בכלל – נוסיף gmail.com
        if (value !== "" && !value.includes("@")) {
            emailInput.value = value + "@gmail.com";
        }
    });
});
