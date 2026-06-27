// FYI Help Here — site.js
(function(){
  'use strict';
  // Prevent double-tap zoom on buttons for mobile
  document.addEventListener('touchend', function(e){
    if(e.target.closest('button,a,.btn,.chip,.sev-opt,.recipient-card,.notify-opt')){
      e.preventDefault();
    }
  }, {passive:false});
})();
