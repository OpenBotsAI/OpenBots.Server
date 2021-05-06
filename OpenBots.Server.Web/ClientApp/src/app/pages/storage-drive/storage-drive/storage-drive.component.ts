import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NbDateService } from '@nebular/theme';
import { HelperService } from '../../../@core/services/helper.service';
import { HttpService } from '../../../@core/services/http.service';
import { StorageDriveApiUrl } from '../../../webApiUrls/storageDriveUrl';

@Component({
  selector: 'ngx-storage-drive',
  templateUrl: './storage-drive.component.html',
  styleUrls: ['./storage-drive.component.scss'],
})
export class StorageDriveComponent implements OnInit {
  storageDriveForm: FormGroup;
  title = 'Add';
  isSubmitted = false;
  urlId: string;
  strageDriveSize = ['MB', 'GB'];
  isMaxMb = false;
  isMaxGb = false;
  eTag: string;
  constructor(
    private fb: FormBuilder,
    private httpService: HttpService,
    private router: Router,
    private route: ActivatedRoute,
    private helperService: HelperService
  ) {}

  ngOnInit(): void {
    this.urlId = this.route.snapshot.params['id'];
    if (this.urlId) {
      this.title = 'Update';
      this.getStorageDriveById();
    }
    this.storageDriveForm = this.initializeForm();
  }
  getStorageDriveById(): void {
    this.httpService
      .get(
        `${StorageDriveApiUrl.storage}/${StorageDriveApiUrl.drives}/${StorageDriveApiUrl.driveDetails}/${this.urlId}`,
        { observe: 'response' }
      )
      .subscribe((response) => {
        if (response) {
          this.eTag = response.headers.get('etag');
          let arr = this.helperService.bytesIntoMBorGB(
            response.body.maxStorageAllowedInBytes
          );
          response.body.maxStorageAllowedInBytes = arr[0];
          this.storageDriveForm.patchValue(response.body);
          this.storageDriveForm.patchValue({
            driveSize: arr[1],
          });
        }
      });
  }
  get formControls() {
    return this.storageDriveForm.controls;
  }

  initializeForm() {
    return this.fb.group({
      // isDeleted: false,

      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
        ],
      ],
      // fileStorageAdapterType: [''],
      // storageSizeInBytes: 0,
      maxStorageAllowedInBytes: [
        ,
        [Validators.max(5000), Validators.pattern(/^-?(0|[1-9]\d*)?$/)],
      ],
      driveSize: ['MB'],
      isDefault: [false],
    });
  }

  onSubmit(): void {
    let storageSize;
    this.isSubmitted = true;
    if (this.storageDriveForm.value.driveSize == 'MB') {
      storageSize = this.helperService.megaBytesIntiBytes(
        +this.storageDriveForm.value.maxStorageAllowedInBytes
      );
    } else if (this.storageDriveForm.value.driveSize == 'GB') {
      storageSize = this.helperService.gegaBytesIntiBytes(
        +this.storageDriveForm.value.maxStorageAllowedInBytes
      );
    }
    delete this.storageDriveForm.value.driveSize;
    const obj = {
      name: this.storageDriveForm.value.name,
      maxStorageAllowedInBytes: storageSize,
      isDefault: false,
    };
    if (this.urlId) this.updateStorageDrive(obj);
    else this.addStorageDrive(obj);
  }
  updateStorageDrive(obj): void {
    const headers = this.helperService.getETagHeaders(this.eTag);
    this.httpService
      .put(
        `${StorageDriveApiUrl.storage}/${StorageDriveApiUrl.drives}/${this.urlId}`,
        obj,
        { observe: 'response', headers }
      )
      .subscribe(
        (response) => {
          if (response && response.status == 200) {
            this.httpService.success('Storage drive updated successfully');
            this.router.navigate([`/pages/storagedrive`]);
          }
        },
        () => (this.isSubmitted = false)
      );
  }
  addStorageDrive(obj): void {
    this.httpService
      .post(`${StorageDriveApiUrl.storage}/${StorageDriveApiUrl.drives}`, obj)
      .subscribe(
        (response) => {
          if (response) {
            this.httpService.success('Storage drive created successfully');
            this.router.navigate([`/pages/storagedrive`]);
          }
        },
        () => (this.isSubmitted = false)
      );
  }

  onChangeDriveSize(event): void {
    if (event == 'GB')
      this.storageDriveForm
        .get('maxStorageAllowedInBytes')
        .setValidators([Validators.max(50)]);
    else if (event == 'MB')
      this.storageDriveForm
        .get('maxStorageAllowedInBytes')
        .setValidators([Validators.max(5000)]);
    this.storageDriveForm
      .get('maxStorageAllowedInBytes')
      .updateValueAndValidity();
    // if (
    //   this.storageDriveForm.value.maxStorageAllowedInBytes > 5000 &&
    //   event == 'MB'
    // )
    //   this.isMaxMb = true;
    // else if (
    //   this.storageDriveForm.value.maxStorageAllowedInBytes > 50 &&
    //   event == 'GB'
    // )
    //   this.isMaxGb = true;
    // else {
    //   this.isMaxGb = false;
    //   this.isMaxMb = false;
    // }
  }
}
