import { Component, OnInit, EventEmitter } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup, FormBuilder, Validators, FormArray } from '@angular/forms';
import { NbToastrService } from '@nebular/theme';
import {
  UploadOutput,
  UploadInput,
  UploadFile,
  UploaderOptions,
} from 'ngx-uploader';
import { AutomationService } from '../automation.service';
import { HttpResponse } from '@angular/common/http';

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
  dragOver: boolean;
  nativeFile: any;
  nativeFileName: any;
  automationSelection: string[] = ['OpenBots', 'Python', 'TagUI', 'CSScript'];
  ///// end declartion////
  etag;
  showAutomation: any = [];
  title = 'Add';
  fileSize = false;
  value = ['Published', 'Commited'];
  showprocess: FormGroup;
  showUpload: boolean = false;
  submitted = false;
  urlId: string;
  dataType = ['Text', 'Number'];
  items: FormArray;

  constructor(
    private formBuilder: FormBuilder,
    private toastrService: NbToastrService,
    protected router: Router,
    private route: ActivatedRoute,
    protected automationService: AutomationService
  ) {
    this.files = [];
    this.uploadInput = new EventEmitter<UploadInput>();
  }

  ngOnInit(): void {
    this.urlId = this.route.snapshot.params['id'];
    if (this.urlId) {
      this.getProcessByID(this.urlId);
      this.title = 'Update';
    }
    this.showprocess = this.initializeShowProcessForm();
  }

  initializeShowProcessForm(): FormGroup {
    return this.formBuilder.group({
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
      automationParameter: this.formBuilder.array([]),
    });
  }
  getProcessByID(id) {
    this.automationService
      .getProcessId(id)
      .subscribe((data: HttpResponse<any>) => {
        this.showAutomation = data.body;
        this.etag = data.headers.get('ETag').replace(/\"/g, '');
        this.showprocess.patchValue(data.body);
      });
  }
  get f() {
    return this.showprocess.controls;
  }

  get formArrayControl() {
    return this.showprocess.get('automationParameter') as FormArray;
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
          this.nativeFile = output.file.nativeFile;
          this.nativeFileName = output.file.nativeFile.name;
          this.showUpload = false;
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
    if (this.urlId) {
      this.updateAutomation();
    } else {
      this.addAutomation();
    }
  }

  addAutomation() {
    this.submitted = true;

    if (this.nativeFile) {
      let AutomationformData = new FormData();
      AutomationformData.append('file', this.nativeFile, this.nativeFileName);
      AutomationformData.append('name', this.showprocess.value.name);
      AutomationformData.append('status', this.showprocess.value.status);
      AutomationformData.append(
        'automationEngine',
        this.showprocess.value.automationEngine
      );
      this.automationService.addProcess(AutomationformData).subscribe(
        (data: any) => {
          this.nativeFileName = undefined;
          this.nativeFile = undefined;
          this.toastrService.success(
            'Automation Add  Successfully!',
            'Success'
          );
          this.router.navigate(['/pages/automation/list']);
        },
        () => (this.submitted = false)
      );
    } else {
      this.showUpload = true;
      this.toastrService.danger('Please Add Automation file!', 'Failed');
      this.submitted = false;
      this.nativeFileName = undefined;
      this.nativeFile = undefined;
    }
  }

  updateAutomation() {
    if (this.nativeFile) {
      let formData = new FormData();
      formData.append('File', this.nativeFile, this.nativeFileName);
      formData.append('name', this.showprocess.value.name);
      formData.append('status', this.showprocess.value.status);
      formData.append(
        'automationEngine',
        this.showprocess.value.automationEngine
      );
      this.automationService
        .uploadUpdateProcessFile(formData, this.urlId, this.etag)
        .subscribe(
          (data: any) => {
            this.showprocess.value.binaryObjectId = data.binaryObjectId;
            this.toastrService.success('Updated successfully', 'Success');
            this.router.navigate(['/pages/automation/list']);
            this.nativeFile = undefined;
            this.nativeFileName = undefined;
          },
          (error) => {
            if (error && error.error && error.error.status === 409) {
              this.toastrService.danger(error.error.serviceErrors, 'error');
              this.getProcessByID(this.urlId);
            }
          }
        );
    } else if (this.nativeFile == undefined) {
      let processobj = {
        name: this.showprocess.value.name,
        status: this.showprocess.value.status,
        automationEngine: this.showprocess.value.automationEngine,
      };
      this.automationService
        .updateProcess(processobj, this.urlId, this.etag)
        .subscribe(
          (data) => {
            this.toastrService.success('Updated successfully', 'Success');
            this.router.navigate(['/pages/automation/list']);
            this.nativeFile = undefined;
            this.nativeFileName = undefined;
          },
          (error) => {
            if (error && error.error && error.error.status === 409) {
              this.toastrService.danger(error.error.serviceErrors, 'error');
              this.getProcessByID(this.urlId);
            }
          }
        );
    }
  }
  automationParameter() {
    this.items = this.showprocess.get('automationParameter') as FormArray;
    this.items.push(this.initializeParameterFormArray());
  }

  initializeParameterFormArray(): FormGroup {
    return this.formBuilder.group({
      Name: ['', [Validators.required]],
      DataType: ['Text', [Validators.required]],
      Value: ['', [Validators.required]],
    });
  }

  deleteAutomationParameter(index: number) {
    this.items.removeAt(index);
  }
}
