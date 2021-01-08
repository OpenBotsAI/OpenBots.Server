import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FileSaverService } from 'ngx-filesaver';
import { FileSizePipe } from 'ngx-filesize';
import { HttpService } from '../../../@core/services/http.service';
import { DialogService } from '../../../@core/dialogservices/dialog.service';
import { TimeDatePipe } from '../../../@core/pipe';
import { HttpResponse } from '@angular/common/http';
import { FilesApiUrl } from '../../../webApiUrls';
import { HelperService } from '../../../@core/services/helper.service';

@Component({
  selector: 'ngx-get-file-id',
  templateUrl: './get-file-id.component.html',
  styleUrls: ['./get-file-id.component.scss'],
})
export class GetFileIdComponent implements OnInit {
  filterOrderBy: string;
  objectViewForm: FormGroup;
  currentUrlId: string;
  pipe: TimeDatePipe;
  show_name: any = [];
  show_del: any = [];
  colRealtionId: string;
  deleteId: string;
  downloadFile: string;
  constructor(
    protected router: Router,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private httpService: HttpService,
    private _FileSaverService: FileSaverService,
    private dialogService: DialogService,
    private helperService: HelperService
  ) { }

  ngOnInit(): void {
    this.currentUrlId = this.route.snapshot.params['id'];
    if (this.currentUrlId) {
      this.getCredentialById();
    }
    this.objectViewForm = this.initizlizeForm();
  }

  initizlizeForm() {
    return this.fb.group({
      contentType: [''],
      correlationEntity: [''],
      correlationEntityId: [''],
      createdBy: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      hashCode: [''],
      folder: [''],
      id: [''],
      isDeleted: [''],
      name: [''],
      organizationId: [''],
      sizeInBytes: [''],
      storagePath: [''],
      storageProvider: [''],
      timestamp: [''],
      updatedBy: [''],
      updatedOn: [''],
    });
  }

  getCredentialById(): void {
    this.httpService
      .get(`${FilesApiUrl.BinaryObjects}/${this.currentUrlId}`, {
        observe: 'response',
      })
      .subscribe((response) => {
        if (response && response.status == 200) {
          this.downloadFile = response.body.sizeInBytes;
          response.body.createdOn = this.helperService.transformDate(
            response.body.createdOn,
            'lll'
          );
          response.body.updatedOn = this.helperService.transformDate(
            response.body.updatedOn,
            'lll'
          );
          let file_size = new FileSizePipe();
          response.body.sizeInBytes = file_size.transform(
            response.body.sizeInBytes
          );
          this.show_del = response.body.correlationEntity;
          this.colRealtionId = response.body.correlationEntityId;
          this.show_name = this.objectViewForm.value.name;
          this.objectViewForm.patchValue({ ...response.body });
          this.objectViewForm.disable();
        }
      });
  }

  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], {
      queryParams: {
        PageName: 'Files',
        id: this.currentUrlId,
      },
    });
  }
  onDown(): void {
    let fileName: string;
    this.httpService
      .get(`${FilesApiUrl.BinaryObjects}/${this.currentUrlId}/download`, {
        observe: 'response',
        responseType: 'blob',
      })
      .subscribe((res: HttpResponse<Blob>) => {
        fileName = res.headers
          .get('content-disposition')
          .split(';')[1]
          .split('=')[1]
          .replace(/\"/g, '');
        this._FileSaverService.save(res.body, fileName);
      });
  }

  openDeleteDialog(ref: TemplateRef<any>): void {
    this.deleteId = this.currentUrlId;
    this.dialogService.openDialog(ref);
  }

  deleteBinaryObjects(ref): void {
    this.httpService
      .delete(`${FilesApiUrl.BinaryObjects}/${this.deleteId}`)
      .subscribe(() => {
        ref.close();
        this.httpService.success('Deleted Successfully');
        this.router.navigate(['/pages/file/list']);
      });
  }
}
