// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


document.addEventListener("DOMContentLoaded", function () {
    // Captura todos os formulários de inscrição da página
    const formularios = document.querySelectorAll(".form-inscricao");

    formularios.forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault(); // Impede a página de recarregar!

            const botao = form.querySelector("button");
            const formData = new FormData(form);

            // Desabilita o botão temporariamente para evitar cliques duplos
            botao.disabled = true;

            // Envia os dados para o seu método "Inscrever" no backend
            fetch(form.action, {
                method: "POST",
                body: formData
            })
            .then(response => {
                if (response.ok) {
                    // Transforma o botão na caixinha verde integrada
                    botao.innerHTML = 'Inscrito <i class="fa-solid fa-check"></i>';
                    botao.classList.add("btn-inscrito-sucesso");
                } else {
                    alert("Ocorreu um erro ao tentar se inscrever.");
                    botao.disabled = false;
                }
            })
            .catch(error => {
                console.error("Erro:", error);
                alert("Erro de conexão.");
                botao.disabled = false;
            });
        });
    });
});