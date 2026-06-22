    document.addEventListener('DOMContentLoaded', function () {
    const bubble = document.getElementById('chatbotBubble');
    const windowEl = document.getElementById('chatbotWindow');
    const closeBtn = document.getElementById('chatbotCloseBtn');
    const sendBtn = document.getElementById('chatbotSendBtn');
    const inputEl = document.getElementById('chatbotInput');
    const bodyEl = document.getElementById('chatbotBody');

    if (!bubble || !windowEl || !closeBtn || !sendBtn || !inputEl || !bodyEl) return;

    // 1. Abrir e Fechar Janela do Chat
    bubble.addEventListener('click', function () {
        windowEl.classList.toggle('active');
        if (windowEl.classList.contains('active')) {
            inputEl.focus();
            // Adiciona mensagem de boas-vindas inicial se o chat estiver vazio
            if (bodyEl.children.length === 0) {
                adicionarMensagem('bot', 'Olá! Sou o assistente do Senai. Como posso te ajudar com as dúvidas sobre o evento Paulo Skaf 2026?');
            }
        }
    });

    closeBtn.addEventListener('click', function () {
        windowEl.classList.remove('active');
    });

    // Fechar ao clicar fora da janela do chat
    document.addEventListener('click', function (e) {
        if (!windowEl.contains(e.target) && !bubble.contains(e.target)) {
            windowEl.classList.remove('active');
        }
    });

    // 2. Enviar Mensagem ao Clicar ou apertar Enter
    sendBtn.addEventListener('click', enviar);
    inputEl.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            enviar();
        }
    });

    async function enviar() {
        const texto = inputEl.value.trim();
        if (!texto) return;

        // Limpa a caixa de entrada
        inputEl.value = '';

        // Adiciona mensagem do usuário
        adicionarMensagem('user', texto);

        // Cria balão de digitando...
        const loadingId = 'loading-' + Date.now();
        adicionarMensagem('bot loading', 'Assistente digitando...', loadingId);

        try {
            // Chamada assíncrona para o novo controlador
            const response = await fetch('/Chat/EnviarMensagem', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ mensagem: texto })
            });

            const data = await response.json();

            // Remove o balão de carregamento e insere a resposta real
            const loadingEl = document.getElementById(loadingId);
            if (loadingEl) loadingEl.remove();

            adicionarMensagem('bot', data.resposta);

        } catch (error) {
            console.error('Erro no Chatbot:', error);
            const loadingEl = document.getElementById(loadingId);
            if (loadingEl) loadingEl.remove();
            adicionarMensagem('bot', 'Desculpe, tive um problema de conexão com o servidor. Pode tentar de novo?');
        }
    }

    function adicionarMensagem(autor, texto, id = null) {
        const msgDiv = document.createElement('div');
        msgDiv.className = `chat-msg ${autor}`;
        if (id) msgDiv.id = id;
        msgDiv.textContent = texto;
        bodyEl.appendChild(msgDiv);

        // Rola automaticamente para o fim da conversa
        bodyEl.scrollTop = bodyEl.scrollHeight;
    }
});