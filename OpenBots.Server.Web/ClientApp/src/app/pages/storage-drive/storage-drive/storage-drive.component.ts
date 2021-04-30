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
  constructor(
    private fb: FormBuilder,
    private httpService: HttpService,
    private dateService: NbDateService<Date>,
    private router: Router,
    private route: ActivatedRoute,
    private helperService: HelperService
  ) {}

  ngOnInit(): void {
    this.storageDriveForm = this.initializeForm();
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
    this.addStorageDrive();
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
