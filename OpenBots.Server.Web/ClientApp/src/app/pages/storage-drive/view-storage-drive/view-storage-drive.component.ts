import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { HelperService } from '../../../@core/services/helper.service';
import { HttpService } from '../../../@core/services/http.service';
import { StorageDriveApiUrl } from '../../../webApiUrls/storageDriveUrl';

@Component({
  selector: 'ngx-view-storage-drive',
  templateUrl: './view-storage-drive.component.html',
  styleUrls: ['./view-storage-drive.component.scss'],
})
export class ViewStorageDriveComponent implements OnInit {
  storageDriveForm: FormGroup;
  urlId: string;
  strageDriveSize = ['MB', 'GB'];
  constructor(
    private fb: FormBuilder,
    private httpService: HttpService,
    private route: ActivatedRoute,
    private helperService: HelperService
  ) {}

  ngOnInit(): void {
    this.urlId = this.route.snapshot.params['id'];
    if (this.urlId) this.getStorageDriveById();

    this.storageDriveForm = this.initializeForm();
  }

  getStorageDriveById(): void {
    this.httpService
      .get(
        `${StorageDriveApiUrl.storage}/${StorageDriveApiUrl.drives}/${StorageDriveApiUrl.driveDetails}/${this.urlId}`
      )
      .subscribe((response) => {
        if (response) {
          let arr = this.helperService.bytesIntoMBorGB(
            response.maxStorageAllowedInBytes
          );
          response.maxStorageAllowedInBytes = arr[0];
          this.storageDriveForm.patchValue(response);
          this.storageDriveForm.patchValue({
            driveSize: arr[1],
          });
          this.storageDriveForm.disable();
        }
      });
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
      maxStorageAllowedInBytes: [''],
      driveSize: ['MB'],
      // isDefault: true,
    });
  }
}
