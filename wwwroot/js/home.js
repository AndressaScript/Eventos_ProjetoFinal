document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('eventModal');
    const closeModalBtn = document.getElementById('closeModalBtn');
    const modalSubscribeBtn = document.getElementById('modalSubscribeBtn');
    const modalSubscribedMessage = document.getElementById('modalSubscribedMessage');
    
    // Elementos do Modal para povoar (Lado Direito)
    const modalEventTitle = document.getElementById('modalEventTitle');
    const modalEventDescription = document.getElementById('modalEventDescription');
    const modalEventImage = document.getElementById('modalEventImage');
    const modalEventDate = document.getElementById('modalEventDate');
    const modalEventLocalFull = document.getElementById('modalEventLocalFull');
    const modalEventPalestranteFull = document.getElementById('modalEventPalestranteFull');
    
    // Elementos do Grid de destaques (Lado Esquerdo)
    const gridEventDate = document.getElementById('gridEventDate');
    const gridEventTime = document.getElementById('gridEventTime');
    const gridEventLocal = document.getElementById('gridEventLocal');
    const gridEventPalestrante = document.getElementById('gridEventPalestrante');

    let activeCard = null;

    // 1. Intercepta cliques no card ou no botão "Saber mais" para abrir o modal
    const eventCards = document.querySelectorAll('.event-card-clickable');
    eventCards.forEach(card => {
        card.addEventListener('click', function (e) {
            // Garante que se clicar no botão "Saber mais" (btn-enroll-trigger), apenas abre o modal
            const trigger = e.target.closest('.btn-enroll-trigger');
            if (trigger) {
                e.preventDefault();
                e.stopPropagation();
            }
            activeCard = card;
            openModal(card);
        });
    });

    function openModal(card) {
        const id = card.dataset.id;
        const nome = card.dataset.nome;
        const data = card.dataset.data;
        const dataFormatada = card.dataset.dataFormatada;
        const hora = card.dataset.hora;
        const horaFormatada = card.dataset.horaFormatada;
        const local = card.dataset.local;
        const palestrante = card.dataset.palestrante;
        const imagem = card.dataset.imagem;
        const descricao = card.dataset.descricao;
        const inscrito = card.dataset.inscrito === 'true';
        const role = card.dataset.role;

        // Povoa modal (Lado Direito)
        modalEventTitle.textContent = nome;
        modalEventDescription.textContent = descricao;
        modalEventImage.src = imagem;
        modalEventImage.alt = nome;
        modalEventDate.textContent = `${dataFormatada} ${horaFormatada}`;
        modalEventLocalFull.textContent = `${local} e Transmissão Online`;
        modalEventPalestranteFull.textContent = `${palestrante}, especialista convidado do SENAI.`;

        // Povoa grid de destaques hexagonais (Lado Esquerdo)
        gridEventDate.textContent = dataFormatada.split(' de ')[0] + ' de ' + dataFormatada.split(' de ')[1]; // Simplifica
        gridEventTime.textContent = horaFormatada;
        gridEventLocal.textContent = local.length > 20 ? local.substring(0, 18) + '...' : local;
        gridEventPalestrante.textContent = palestrante;

        // Guarda os IDs no botão do modal
        modalSubscribeBtn.dataset.eventoId = id;
        modalSubscribeBtn.dataset.role = role;

        // Gerencia estado de exibição do botão/mensagem de inscrição
        if (inscrito) {
            modalSubscribeBtn.style.display = 'none';
            modalSubscribedMessage.style.display = 'flex';
        } else {
            modalSubscribedMessage.style.display = 'none';
            if (role === 'Admin') {
                modalSubscribeBtn.style.display = 'none'; // Admin não se inscreve
            } else {
                modalSubscribeBtn.style.display = 'flex';
                modalSubscribeBtn.innerHTML = '<span>Inscrever-se</span> <i class="fa-solid fa-arrow-right"></i>';
                modalSubscribeBtn.disabled = false;
            }
        }

        // Mostra modal com animação suave
        modal.style.display = 'flex';
        setTimeout(() => {
            modal.classList.add('active');
        }, 10);
        
        document.body.style.overflow = 'hidden';
    }

    function closeModalFunc() {
        modal.classList.remove('active');
        setTimeout(() => {
            modal.style.display = 'none';
        }, 300);
        document.body.style.overflow = '';
        activeCard = null;
    }

    if (closeModalBtn) {
        closeModalBtn.addEventListener('click', closeModalFunc);
    }
    
    modal.addEventListener('click', function (e) {
        if (e.target === modal) {
            closeModalFunc();
        }
    });

    // 2. Processa a inscrição via AJAX ao clicar no botão de dentro do Modal
    modalSubscribeBtn.addEventListener('click', function () {
        const eventoId = modalSubscribeBtn.dataset.eventoId;
        const role = modalSubscribeBtn.dataset.role;

        // Redireciona se não estiver logado como Aluno
        if (!role || role !== 'Aluno') {
            window.location.href = '/Login';
            return;
        }

        modalSubscribeBtn.disabled = true;
        modalSubscribeBtn.innerHTML = '<span>Processando...</span> <i class="fa-solid fa-spinner fa-spin"></i>';

        const formData = new FormData();
        formData.append('eventoId', eventoId);

        fetch('/Home/InscreverAjax', {
            method: 'POST',
            body: formData
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // EXIBE O ALERTA solicitado pelo usuário
                alert("Inscrito com sucesso!");

                // Atualiza visual do Modal imediatamente
                modalSubscribeBtn.style.display = 'none';
                modalSubscribedMessage.style.display = 'flex';

                // Atualiza o Card físico na tela de fundo (Fim do Saber mais, bloqueia nova inscrição)
                if (activeCard) {
                    activeCard.dataset.inscrito = 'true';
                    
                    const actionWrapper = activeCard.querySelector('.card-footer-action');
                    if (actionWrapper) {
                        actionWrapper.innerHTML = <span class="btn-subscribe-disabled" style="display: block; text-align: center; color: #22c55e; font-weight: bold;"><i class="fa-solid fa-circle-check"></i> Inscrito</span>;
                    }
                }
            } else {
                if (data.redirectUrl) {
                    window.location.href = data.redirectUrl;
                } else {
                    alert(data.message || 'Erro ao realizar inscrição.');
                    modalSubscribeBtn.disabled = false;
                    modalSubscribeBtn.innerHTML = '<span>Inscrever-se</span> <i class="fa-solid fa-arrow-right"></i>';
                }
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Erro de conexão ao tentar se inscrever.');
            modalSubscribeBtn.disabled = false;
            modalSubscribeBtn.innerHTML = '<span>Inscrever-se</span> <i class="fa-solid fa-arrow-right"></i>';
        });
    });

    // ==========================================================
    // 3. LÓGICA DO CARROSSEL DE PRÓXIMOS EVENTOS (RESPONSIVO)
    // ==========================================================
    const track = document.getElementById('carouselTrack');
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    
    if (track && prevBtn && nextBtn) {
        const items = track.querySelectorAll('.carousel-item-new');
        const totalItems = items.length;
        let currentIndex = 0;
        
        function getItemsToShow() {
            if (window.innerWidth <= 640) return 1;
            if (window.innerWidth <= 1024) return 2;
            return 3;
        }
        
        let itemsToShow = getItemsToShow();
        
        if (totalItems <= itemsToShow) {
            prevBtn.style.display = 'none';
            nextBtn.style.display = 'none';
        } else {
            updateCarouselButtons();
            
            nextBtn.addEventListener('click', () => {
                if (currentIndex < totalItems - itemsToShow) {
                    currentIndex++;
                    slideCarousel();
                }
            });
            
            prevBtn.addEventListener('click', () => {
                if (currentIndex > 0) {
                    currentIndex--;
                    slideCarousel();
                }
            });
        }
        
        function slideCarousel() {
            if (totalItems === 0) return;
            const itemWidth = items[0].getBoundingClientRect().width;
            const trackStyle = window.getComputedStyle(track);
            const gap = parseFloat(trackStyle.gap) || 0;
            const slideAmount = itemWidth + gap;
            
            track.style.transform = `translateX(-${currentIndex * slideAmount}px)`;
            updateCarouselButtons();
        }
        
        function updateCarouselButtons() {
            prevBtn.disabled = currentIndex === 0;
            nextBtn.disabled = currentIndex >= totalItems - itemsToShow;
        }
        
        window.addEventListener('resize', () => {
            itemsToShow = getItemsToShow();
            
            if (totalItems <= itemsToShow) {
                prevBtn.style.display = 'none';
                nextBtn.style.display = 'none';
                track.style.transform = 'translateX(0px)';
                currentIndex = 0;
            } else {
                prevBtn.style.display = 'flex';
                nextBtn.style.display = 'flex';
                
                if (currentIndex > totalItems - itemsToShow) {
                    currentIndex = Math.max(0, totalItems - itemsToShow);
                }
                slideCarousel();
            }
        });
    }
});