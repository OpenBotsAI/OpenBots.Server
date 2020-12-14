import { Component, OnInit, EventEmitter } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AutomationService } from '../automation.service';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NbToastrService } from '@nebular/theme';
import {
  UploadOutput,
  UploadInput,
  UploadFile,
  humanizeBytes,
  UploaderOptions,
} from 'ngx-uploader';
import { HttpResponse } from '@angular/common/http';

@Component({
  selector: 'ngx-edit-automation',
  templateUrl: './edit-automation.component.html',
  styleUrls: ['./edit-automation.component.scss'],
})
export class EditAutomationComponent implements OnInit {
  //// file upload declartion ////
  options: UploaderOptions;
  files: UploadFile[];
  automationSelection: string[] = ['OpenBots', 'Python'];
  uploadInput: EventEmitter<UploadInput>;
  humanizeBytes: Function;
  dragOver: boolean;
  native_file: any;
  native_file_name: any;
  ///// end declartion////
  fileSize = false;
  value = ['Published', 'Commited'];
  showprocess: FormGroup;
  save_value: any = [];
  show_upload: boolean = false;
  submitted = false;
  process_id: any = [];
  show_allprocess: any = [];
  fileUploadId;
  etag;
  constructor(
    private formBuilder: FormBuilder,
    private toastrService: NbToastrService,
    protected router: Router,
    protected automationService: AutomationService,
    private acroute: ActivatedRoute
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.process_id = params.id;
      this.get_process(params.id);
    });
  }

  get_process(id) {
    this.automationService
      .getProcessId(id)
      .subscribe((data: HttpResponse<any>) => {
        this.show_allprocess = data.body;
        this.etag = data.headers.get('ETag').replace(/\"/g, '');
        this.showprocess.patchValue(data.body);
      });
  }

  ngOnInit(): void {
    this.showprocess = this.formBuilder.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(2),
          Validators.maxLength(100),
          Validators.pattern('^[A-Za-z0-9_.-]{3,100}$'),
        ],
      ],
      status: [''],
      automationEngine: [''],
    });
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
        }
        break;
    }
  }

  get f() {
    return this.showprocess.controls;
  }

  onSubmit() {
    if (this.native_file) {
      let formData = new FormData();
      formData.append('file', this.native_file, this.native_file_name);
      formData.append('name', this.showprocess.value.name);
      formData.append('status', this.showprocess.value.status);
      formData.append(
        'automationEngine',
        this.showprocess.value.automationEngine
      );
      this.automationService
        .uploadUpdateProcessFile(formData, this.process_id, this.etag)
        .subscribe(
          (data: any) => {
            this.showprocess.value.binaryObjectId = data.binaryObjectId;
            this.toastrService.success('Updated successfully', 'Success');
            this.router.navigate(['/pages/automation/list']);
            this.native_file = undefined;
            this.native_file_name = undefined;
          },
          (error) => {
            if (error && error.error && error.error.status === 409) {
              this.toastrService.danger(error.error.serviceErrors, 'error');
              this.get_process(this.process_id);
            }
          }
        );
    } else if (this.native_file == undefined) {
      let processobj = {
        name: this.showprocess.value.name,
        status: this.showprocess.value.status,
        automationEngine: this.showprocess.value.automationEngine,
      };
      this.automationService
        .updateProcess(processobj, this.process_id, this.etag)
        .subscribe(
          (data) => {
            this.toastrService.success('Updated successfully', 'Success');
            this.router.navigate(['/pages/automation/list']);
            this.native_file = undefined;
            this.native_file_name = undefined;
          },
          (error) => {
            if (error && error.error && error.error.status === 409) {
              this.toastrService.danger(error.error.serviceErrors, 'error');
              this.get_process(this.process_id);
            }
          }
        );
    }
  }
}
