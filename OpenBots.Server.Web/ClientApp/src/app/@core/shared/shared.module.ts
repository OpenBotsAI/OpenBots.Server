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
import { TimeDatePipe, ChevronPipe, OrderByPipe } from '../pipe';
import { NgJsonEditorModule } from 'ang-jsoneditor';
import { NgxUploaderModule } from 'ngx-uploader';
import { FileSizePipe } from '../pipe/filesize.pipe';
import { NgxFilesizeModule } from 'ngx-filesize';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PreventSpecialCharDirective } from '../../@directive/prevent-special-char.directive';
import { InputTrimModule } from 'ng2-trim-directive';
import { TimeagoPipe } from '../services/timeago.pipe';
import { TooltipComponent } from './tooltip/tooltip.component';
import { BlockUIModule } from 'ng-block-ui';

@NgModule({
  declarations: [
    TimeDatePipe,
    ChevronPipe,
    FileSizePipe,
    TooltipComponent,
    PreventSpecialCharDirective,
    TimeagoPipe,
    OrderByPipe,
  ],
  imports: [NbTooltipModule],
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
    BlockUIModule,
    OrderByPipe,
  ],
})
export class SharedModule {}
