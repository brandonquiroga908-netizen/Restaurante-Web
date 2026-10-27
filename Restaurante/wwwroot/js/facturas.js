document.getElementById('imprimir-factura')?.addEventListener('click', () => window.print());
if (document.querySelector('[data-imprimir="true"]')) {
    window.addEventListener('load', () => window.print());
}
