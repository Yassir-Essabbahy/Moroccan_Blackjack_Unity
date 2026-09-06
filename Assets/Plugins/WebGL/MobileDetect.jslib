mergeInto(LibraryManager.library, {
    IsMobileBrowserJS: function () {
        var ua = navigator.userAgent || navigator.vendor || window.opera || "";
        var isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Mobile/i.test(ua);
        var isIPad = (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
        return (isMobile || isIPad) ? 1 : 0;
    }
});
