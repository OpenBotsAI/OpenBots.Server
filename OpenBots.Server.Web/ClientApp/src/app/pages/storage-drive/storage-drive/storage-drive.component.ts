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
  title = 'add';
  isSubmitted = false;
  urlId: string;
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
      storageSizeInBytes: [''],
      // maxStorageAllowedInBytes: 0,
      // isDefault: true,
    });
  }

  onSubmit(): void {
    this.isSubmitted = true;
    if (this.urlId) this.updateStorageDrive();
    else this.addStorageDrive();
  }
  updateStorageDrive(): void {
    this.httpService
      .put(
        `${StorageDriveApiUrl.storage}/${StorageDriveApiUrl.drives}/${this.urlId}`,
        this.storageDriveForm.value
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
  addStorageDrive(): void {
    this.httpService
      .post(
        `${StorageDriveApiUrl.storage}/${StorageDriveApiUrl.drives}`,
        this.storageDriveForm.value
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
}
