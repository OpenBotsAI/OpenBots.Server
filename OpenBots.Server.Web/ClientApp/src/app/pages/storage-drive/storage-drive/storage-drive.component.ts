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
  strageDriveSize = ['GB'];
  isMaxMb = false;
  isMaxGb = false;
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
        `${StorageDriveApiUrl.storage}/${StorageDriveApiUrl.drives}/${StorageDriveApiUrl.driveDetails}/${this.urlId}`
      )
      .subscribe((response) => {
        if (response) {
          response.maxStorageAllowedInBytes = (
            response.maxStorageAllowedInBytes /
            (1024 * 1024 * 1024)
          ).toFixed(2);
          this.storageDriveForm.patchValue(response);
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
      maxStorageAllowedInBytes: null,
      driveSize: ['GB'],
      isDefault: [false],
    });
  }

  onSubmit(): void {
    this.isSubmitted = true;
    // if (this.storageDriveForm.value.driveSize == 'MB') {
    //   this.storageDriveForm.value.maxStorageAllowedInBytes = (
    //     this.storageDriveForm.value.maxStorageAllowedInBytes *
    //     (1024 * 1024)
    //   ).toFixed(2);
    // } else
    let value;
    if (this.storageDriveForm.value.driveSize == 'GB') {
      value = this.storageDriveForm.value.maxStorageAllowedInBytes;
      console.log('before', value);
      value = value * 1024 * 1024 * 1024;
      console.log('after', value);
      this.storageDriveForm.value.maxStorageAllowedInBytes = value;
      // this.storageDriveForm.value.maxStorageAllowedInBytes = +this
      //   .storageDriveForm.value.maxStorageAllowedInBytes;
    }
    if (!this.storageDriveForm.value.isDefault) {
      this.storageDriveForm.get('isDefault').setValue(false);
    }
    console.log('values', this.storageDriveForm.value);
    delete this.storageDriveForm.value.driveSize;
    const obj = {
      name: this.storageDriveForm.value.name,
      maxStorageAllowedInBytes: value,
      isDefault: false,
    };
    if (this.urlId) this.updateStorageDrive(obj);
    else this.addStorageDrive(obj);
  }
  updateStorageDrive(obj): void {
    this.httpService
      .put(
        `${StorageDriveApiUrl.storage}/${StorageDriveApiUrl.drives}/${this.urlId}`,
        obj
      )
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
  onChangesValue(event) {
    if (
      event.target.value > 5000 &&
      this.storageDriveForm.value.driveSize == 'MB'
    ) {
      this.isMaxMb = true;
    } else if (
      event.target.value > 50 &&
      this.storageDriveForm.value.driveSize == 'GB'
    ) {
      this.isMaxGb = true;
    } else {
      this.isMaxGb = false;
      this.isMaxMb = false;
    }
  }
}
