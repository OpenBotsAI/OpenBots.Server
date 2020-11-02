import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  NbButtonModule,
  NbCardModule,
  NbInputModule,
  NbRadioModule,
  NbIconModule,
  NbSpinnerModule,
  NbDatepickerModule,
  NbDialogModule,
  NbTooltipModule,
  NbCheckboxModule,
} from '@nebular/theme';
import { TimeDatePipe, ChevronPipe } from '../pipe';
import { NgJsonEditorModule } from 'ang-jsoneditor';
import { NgxUploaderModule } from 'ngx-uploader';
import { FileSizePipe } from '../pipe/filesize.pipe';
import { NgxFilesizeModule } from 'ngx-filesize';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TooltipComponent } from '../../../tooltip/tooltip/tooltip.component';
import { PreventSpecialCharDirective } from '../../@directive/prevent-special-char.directive';
import { InputTrimModule } from 'ng2-trim-directive';
import { TimeagoPipe } from '../services/timeago.pipe';

@NgModule({
  declarations: [
    TimeDatePipe,
    ChevronPipe,
    FileSizePipe,
    TooltipComponent,
    PreventSpecialCharDirective,
    TimeagoPipe,
  ],
  imports: [
    NbTooltipModule,
  ],
  exports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NbTooltipModule,
    NbButtonModule,
    NbCardModule,
    NbInputModule,
    NbRadioModule,
    NbIconModule,
    NbCheckboxModule,
    NbSpinnerModule,
    NbDatepickerModule,
    TimeDatePipe,
    FileSizePipe,
    NbDialogModule,
    ChevronPipe,
    NgJsonEditorModule,
    NgxUploaderModule,
    NgxFilesizeModule,
    TooltipComponent,
    PreventSpecialCharDirective,
    InputTrimModule,
    TimeagoPipe,
  ],
})
export class SharedModule {}
