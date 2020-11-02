import { Injectable, TemplateRef } from '@angular/core';
import { NbDialogService } from '@nebular/theme';

@Injectable({
  providedIn: 'root',
})
export class DialogService {
  constructor(private dialogService: NbDialogService) {}

  openDialog(ref: TemplateRef<any>) {
    this.dialogService.open(ref, {
      hasBackdrop: true,
      closeOnBackdropClick: false,
    });
  }
}
