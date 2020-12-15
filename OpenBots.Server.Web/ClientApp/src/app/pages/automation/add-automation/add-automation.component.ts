import { Component, OnInit, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';

import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NbToastrService } from '@nebular/theme';
import {
  UploadOutput,
  UploadInput,
  UploadFile,
  humanizeBytes,
  UploaderOptions,
} from 'ngx-uploader';
import { AutomationService } from '../automation.service';

@Component({
  selector: 'ngx-add-automation',
  templateUrl: './add-automation.component.html',
  styleUrls: ['./add-automation.component.scss'],
})
export class AddAutomationComponent implements OnInit {
  //// file upload declartion ////
  options: UploaderOptions;
  files: UploadFile[];
  uploadInput: EventEmitter<UploadInput>;
  humanizeBytes: Function;
  dragOver: boolean;
  native_file: any;
  native_file_name: any;
  automationSelection: string[] = ['OpenBots', 'Python'];
  ///// end declartion////
  fileSize = false;
  value = ['Published', 'Commited'];
  showprocess: FormGroup;
  save_value: any = [];
  show_upload: boolean = false;
  submitted = false;

  constructor(
    private formBuilder: FormBuilder,
    private toastrService: NbToastrService,
    protected router: Router,
    protected automationService: AutomationService
  ) {
    this.files = [];
    this.uploadInput = new EventEmitter<UploadInput>();
    this.humanizeBytes = humanizeBytes;
  }

  ngOnInit(): void {
    this.showprocess = this.formBuilder.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
          Validators.pattern('^[A-Za-z0-9_.-]{3,100}$'),
        ],
      ],
      status: ['Published'],
      automationEngine: [''],
    });
  }

  get f() {
    return this.showprocess.controls;
  }

  onUploadOutput(output: UploadOutput): void {
    switch (output.type) {
      case 'addedToQueue':
        if (typeof output.file !== 'undefined') {
          if (!output.file.size) {
            this.fileSize = true;
            this.submitted = true;
          } else {
            this.fileSize = false;
            this.submitted = false;
          }
          this.native_file = output.file.nativeFile;
          this.native_file_name = output.file.nativeFile.name;
          this.show_upload = false;
        }
        break;
    }
  }

  cancelUpload(id: string): void {
    this.uploadInput.emit({ type: 'cancel', id: id });
  }

  removeFile(id: string): void {
    this.uploadInput.emit({ type: 'remove', id: id });
  }

  removeAllFiles(): void {
    this.uploadInput.emit({ type: 'removeAll' });
  }
  onSubmit() {
    this.submitted = true;
    let formData = new FormData();
    if (this.native_file) {
      formData.append('file', this.native_file, this.native_file_name);

      let processobj = {
        name: this.showprocess.value.name,
        status: this.showprocess.value.status,
        automationEngine: this.showprocess.value.automationEngine,
      };

      this.automationService.addProcess(processobj).subscribe(
        (data: any) => {
          this.automationService.uploadProcessFile(data.id, formData).subscribe(
            (filedata: any) => {
              this.native_file_name = undefined;
              this.native_file = undefined;
              this.toastrService.success(
                'Automation Add  Successfully!',
                'Success'
              );
              this.router.navigate(['/pages/automation/list']);
            },
            () => (this.submitted = false)
          );
        },
        () => (this.submitted = false)
      );
    } else {
      this.show_upload = true;
      this.toastrService.danger('Please Add Automation file!', 'Failed');
      this.submitted = false;
      this.native_file_name = undefined;
      this.native_file = undefined;
    }
  }
}
