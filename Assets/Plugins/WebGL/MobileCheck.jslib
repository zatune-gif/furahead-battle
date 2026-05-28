mergeInto(LibraryManager.library, {

    JS_IsMobileBrowser: function() {
        return /Android|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) ? 1 : 0;
    },

    JS_IsFBBrowser: function() {
        return /FBAN|FBAV|FB_IAB|FB4A/i.test(navigator.userAgent) ? 1 : 0;
    },

    JS_IsPortrait: function() {
        if (typeof screen !== 'undefined' && screen.orientation && screen.orientation.type) {
            return screen.orientation.type.includes('portrait') ? 1 : 0;
        }
        return window.innerHeight > window.innerWidth ? 1 : 0;
    },

    JS_LockLandscape: function() {
        if (typeof screen !== 'undefined' && screen.orientation && screen.orientation.lock) {
            screen.orientation.lock('landscape').catch(function(e) {});
        }
    },

    JS_SetupMobileCanvas: function() {
        // モバイルUAでない場合は即リターン（PC保護）
        if (!/Android|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) return;

        var canvas = document.querySelector('#unity-canvas') || document.querySelector('canvas');
        if (!canvas) return;

        // モバイルのみ: キャンバスをビューポート全体に固定
        canvas.style.position = 'fixed';
        canvas.style.left     = '0';
        canvas.style.top      = '0';

        function fitCanvas() {
            canvas.style.width  = window.innerWidth  + 'px';
            canvas.style.height = window.innerHeight + 'px';
        }

        window.addEventListener('resize', function() { setTimeout(fitCanvas, 200); });
        window.addEventListener('orientationchange', function() { setTimeout(fitCanvas, 400); });
        fitCanvas();
    },

    JS_SetupAudioUnlock: function() {
        function tryResume() {
            // Unity 6 WebGL audio context
            if (typeof WEBAudio !== 'undefined' && WEBAudio.audioContext &&
                WEBAudio.audioContext.state === 'suspended') {
                WEBAudio.audioContext.resume();
            }
        }
        document.addEventListener('touchstart', tryResume, { passive: true });
        document.addEventListener('touchend',   tryResume, { passive: true });
        document.addEventListener('click',      tryResume);
    }

});
