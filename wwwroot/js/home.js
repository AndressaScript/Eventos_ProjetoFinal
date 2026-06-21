document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('eventModal');
    const closeModalBtn = document.getElementById('closeModalBtn');
    const modalSubscribeBtn = document.getElementById('modalSubscribeBtn');
    const modalSubscribedMessage = document.getElementById('modalSubscribedMessage');
    
    // Elementos do Modal para povoar
    const modalEventTitle = document.getElementById('modalEventTitle');
    const modalEventDescription = document.getElementById('modalEventDescription');
    const modalEventImage = document.getElementById('modalEventImage');
    const modalEventDate = document.getElementById('modalEventDate');
    const modalEventLocal = document.getElementById('modalEventLocal');
    const modalEventPalestrante = document.getElementById('modalEventPalestrante');
    
    // Elementos do Grid de especificidades no Modal
    const gridEventDate = document.getElementById('gridEventDate');
    const gridEventLocal = document.getElementById('gridEventLocal');
    const gridEventPalestrante = document.getElementById('gridEventPalestrante');
    const gridEventType = document.getElementById('gridEventType');

    let activeCard = null;

    // 1. Intercepta cliques no card para abrir o modal de detalhes
    const eventCards = document.querySelectorAll('.event-card-clickable');
    eventCards.forEach(card => {
        card.addEventListener('click', function (e) {
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
        const hora = card.dataset.hora;
        const local = card.dataset.local;
        const palestrante = card.dataset.palestrante;
        const imagem = card.dataset.imagem;
        const descricao = card.dataset.descricao;
        const inscrito = card.dataset.inscrito === 'true';
        const role = card.dataset.role;

        // Povoa modal
        modalEventTitle.textContent = nome;
        modalEventDescription.textContent = descricao;
        modalEventImage.src = imagem;
        modalEventImage.alt = nome;
        modalEventDate.textContent = `${data} às ${hora}`;
        modalEventLocal.textContent = local;
        modalEventPalestrante.textContent = palestrante;

        // Povoa grid interno do modal
        gridEventDate.textContent = data;
        gridEventLocal.textContent = local;
        gridEventPalestrante.textContent = palestrante;
        gridEventType.textContent = 'Evento Presencial';

        modalSubscribeBtn.dataset.eventoId = id;
        modalSubscribeBtn.dataset.role = role;

        // Gerencia estado de inscrição
        if (inscrito) {
            modalSubscribeBtn.style.display = 'none';
            modalSubscribedMessage.style.display = 'flex';
        } else {
            modalSubscribedMessage.style.display = 'none';
            if (role === 'Admin') {
                modalSubscribeBtn.style.display = 'none';
            } else {
                modalSubscribeBtn.style.display = 'flex';
                modalSubscribeBtn.innerHTML = 'Inscrever-se <i class="fa-solid fa-arrow-right"></i>';
                modalSubscribeBtn.disabled = false;
            }
        }

        // Mostra modal com animação de fade-in
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

    // 2. Processa a inscrição via AJAX
    modalSubscribeBtn.addEventListener('click', function () {
        const eventoId = modalSubscribeBtn.dataset.eventoId;
        const role = modalSubscribeBtn.dataset.role;

        if (!role || role !== 'Aluno') {
            window.location.href = '/Login';
            return;
        }

        modalSubscribeBtn.disabled = true;
        modalSubscribeBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Processando...';

        const formData = new FormData();
        formData.append('eventoId', eventoId);

        fetch('/Home/InscreverAjax', {
            method: 'POST',
            body: formData
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Atualiza visual do Modal
                modalSubscribeBtn.style.display = 'none';
                modalSubscribedMessage.style.display = 'flex';

                // Atualiza o Card físico na Home ou página Todos (Fim da recarga de tela!)
                if (activeCard) {
                    activeCard.dataset.inscrito = 'true';
                    
                    // Suporta os dois layouts (com wrapper antigo ou novo footer-action)
                    const actionWrapper = activeCard.querySelector('.card-action-wrapper') || activeCard.querySelector('.card-footer-action');
                    if (actionWrapper) {
                        actionWrapper.innerHTML = `<span class="btn-subscribe-disabled" style="display: block; text-align: center; color: #22c55e; font-weight: bold;"><i class="fa-solid fa-circle-check"></i> Inscrito</span>`;
                    }
                }
            } else {
                if (data.redirectUrl) {
                    window.location.href = data.redirectUrl;
                } else {
                    alert(data.message || 'Erro ao realizar inscrição.');
                    modalSubscribeBtn.disabled = false;
                    modalSubscribeBtn.innerHTML = 'Inscrever-se <i class="fa-solid fa-arrow-right"></i>';
                }
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Erro de conexão ao tentar se inscrever.');
            modalSubscribeBtn.disabled = false;
            modalSubscribeBtn.innerHTML = 'Inscrever-se <i class="fa-solid fa-arrow-right"></i>';
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
        
        // Define dinamicamente o número de cards exibidos
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
            
            track.style.transform = 'translateX(-' + (currentIndex * slideAmount) + 'px)';
            updateCarouselButtons();
        }
        
        function updateCarouselButtons() {
            prevBtn.disabled = currentIndex === 0;
            nextBtn.disabled = currentIndex >= totalItems - itemsToShow;
        }
        
        // Mantém o alinhamento correto no redimensionamento da tela
        window.addEventListener('resize', () => {
            const oldItemsToShow = itemsToShow;
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