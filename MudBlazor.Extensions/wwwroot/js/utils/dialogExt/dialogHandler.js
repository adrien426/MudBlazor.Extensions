﻿class MudExDialogHandler extends MudExDialogHandlerBase {

    handle(dialog) {
        super.handle(dialog);
        setTimeout(() => dialog.classList.remove('mud-ex-dialog-initial'), 50);
        dialog.__extended = true;
        dialog.setAttribute('data-mud-extended', true);
        dialog.classList.add('mud-ex-dialog');
        this.handleAll(dialog);
        if (this.onDone) this.onDone();
    }
    
}

window.MudExDialogHandler = MudExDialogHandler;