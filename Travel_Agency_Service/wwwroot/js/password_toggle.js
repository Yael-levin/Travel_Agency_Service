document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('.toggle-password').forEach(btn => {
            btn.addEventListener('click', () => {
                const input = btn.closest('.input-group')?.querySelector('input');
                if (!input) return;

                if (input.type === 'password') {
                    input.type = 'text';
                    btn.innerText = '🙈';
                } else {
                    input.type = 'password';
                    btn.innerText = '👁️';
                }
            });
        });
});
