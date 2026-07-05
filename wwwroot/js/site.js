// ===== Selector DNI/CE compartido =====
(function () {
    document.querySelectorAll('form').forEach(function (formulario) {
        const tipoDocumento = formulario.querySelector('input[name="TipoDocumento"]');
        const usuario = formulario.querySelector('input[name="Usuario"], input[name="NumeroDocumento"]');
        if (!tipoDocumento || !usuario) return;

        formulario.querySelectorAll('.btn-doc').forEach(function (btn) {
            btn.addEventListener('click', function () {
                formulario.querySelectorAll('.btn-doc').forEach(b => b.classList.remove('activo'));
                btn.classList.add('activo');
                tipoDocumento.value = btn.dataset.tipo;
                usuario.maxLength = btn.dataset.tipo === 'DNI' ? 8 : 9;
                usuario.value = '';
                usuario.focus();
                usuario.dispatchEvent(new Event('input', { bubbles: true }));
            });
        });

        usuario.addEventListener('input', function () {
            if (tipoDocumento.value === 'DNI') {
                usuario.value = usuario.value.replace(/\D/g, '');
            }
        });
    });
})();

// ===== Página de login: mostrar contraseña y estado del botón =====
(function () {
    const formLogin = document.getElementById('form-login');
    if (!formLogin) return;

    const usuario = formLogin.querySelector('input[name="Usuario"]');
    const contrasena = formLogin.querySelector('input[name="Contrasena"]');
    const btnIngresar = document.getElementById('btnIngresar');

    usuario.addEventListener('input', actualizarBoton);
    contrasena.addEventListener('input', actualizarBoton);

    function actualizarBoton() {
        btnIngresar.disabled = usuario.value.trim() === '' || contrasena.value === '';
    }
    actualizarBoton();

    // Mostrar / ocultar contraseña
    const toggle = document.getElementById('toggleContrasena');
    toggle.addEventListener('click', function () {
        contrasena.type = contrasena.type === 'password' ? 'text' : 'password';
    });

    // El aviso de sesión expirada desaparece a los 8 segundos
    const aviso = document.querySelector('.aviso-expiracion');
    if (aviso) setTimeout(() => aviso.remove(), 8000);
})();

// ===== Control de expiración de sesión por inactividad =====
(function () {
    const body = document.body;
    if (body.dataset.autenticado !== '1') return;

    const minutosSesion = parseInt(body.dataset.sesionMinutos, 10);
    const avisoSegundos = parseInt(body.dataset.avisoSegundos, 10);
    const msHastaAviso = minutosSesion * 60 * 1000 - avisoSegundos * 1000;

    const modalElemento = document.getElementById('modalSesion');
    const modal = new bootstrap.Modal(modalElemento);
    const contador = document.getElementById('segundosRestantes');
    const formLogout = document.getElementById('form-logout');

    let temporizadorAviso = null;
    let temporizadorCuenta = null;

    function iniciarTemporizador() {
        clearTimeout(temporizadorAviso);
        temporizadorAviso = setTimeout(mostrarAviso, msHastaAviso);
    }

    function mostrarAviso() {
        let restantes = avisoSegundos;
        contador.textContent = restantes;
        modal.show();

        temporizadorCuenta = setInterval(function () {
            restantes--;
            contador.textContent = restantes;
            if (restantes <= 0) {
                clearInterval(temporizadorCuenta);
                cerrarPorInactividad();
            }
        }, 1000);
    }

    function cerrarPorInactividad() {
        // La cookie ya expiró en el servidor: se usa un GET dedicado en lugar del
        // formulario de logout, cuyo token antiforgery dejaría de validar (400).
        window.location.href = '/Cuenta/SesionExpirada';
    }

    // "Extender sesión": renueva la cookie en el servidor y reinicia el temporizador
    document.getElementById('btnExtenderSesion').addEventListener('click', function () {
        const token = formLogout.querySelector('input[name="__RequestVerificationToken"]').value;
        fetch('/Cuenta/ExtenderSesion', {
            method: 'POST',
            headers: { 'RequestVerificationToken': token }
        }).then(function (respuesta) {
            if (respuesta.ok) {
                clearInterval(temporizadorCuenta);
                modal.hide();
                iniciarTemporizador();
            }
        });
    });

    iniciarTemporizador();
})();
