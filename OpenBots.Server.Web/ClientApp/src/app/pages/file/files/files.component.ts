import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpService } from '../../../@core/services/http.service';
import { BinaryObjects } from '../../../interfaces/binaryObjects';
import { Page } from '../../../interfaces/paginateInstance';
import { DialogService } from '../../../@core/dialogservices/dialog.service';
import { HelperService } from '../../../@core/services/helper.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';

@Component({
  selector: 'ngx-files',
  templateUrl: './files.component.html',
  styleUrls: ['./files.component.scss'],
})
export class FileComponent implements OnInit {
  binaryObjectsForm: FormGroup;
  binaryObjectsData: BinaryObjects[] = [];
  page: Page = {};
  deleteId: string;
  filterOrderBy: string;
  itemsPerPage: ItemsPerPage[] = [];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private helperService: HelperService,
    private httpService: HttpService,
    private dialogService: DialogService
  ) {}

  ngOnInit(): void {
    this.binaryObjectsForm = this.initializeBinaryForm();
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.pagination(this.page.pageNumber, this.page.pageSize);
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  initializeBinaryForm() {
    return this.fb.group({
      itemsPerPage: 5,
      name: [''],
      organizationId: [''],
      contentType: [''],
      corelationEntityId: [''],
      corelationEntity: [''],
      storagePath: [''],
      storageProvider: [''],
      sizeInBytes: [],
      hashCode: [''],
      id: [''],
      isDeleted: [],
      createdBy: [''],
      createdOn: [''],
      deletedBy: [''],
      deleteOn: [''],
      timestamp: [''],
      updatedOn: [''],
      updatedBy: [''],
    });
  }

  getAllBinaryObjects(top, skip, orderBy?): void {
    let url: string;
    orderBy
      ? (url = `BinaryObjects?$top=${top}&$skip=${skip}&$orderby=${orderBy}`)
      : (url = `BinaryObjects?$top=${top}&$skip=${skip}&$orderby=createdOn+desc`);

    this.httpService.get(url).subscribe((response) => {
      response && response.items && response.items.length !== 0
        ? (this.binaryObjectsData = [...response.items])
        : (this.binaryObjectsData = []);
      this.page.totalCount = response.totalCount;
    });
  }

  viewObject(id: string) {
    this.router.navigate([`/pages/file/get-file-id/${id}`]);
  }
  gotoadd() {
    this.router.navigate(['/pages/file/add']);
  }

  openDeleteDialog(ref: TemplateRef<any>, id: string): void {
    this.deleteId = id;
    this.dialogService.openDialog(ref);
  }

  deleteBinaryObjects(ref): void {
    this.httpService.delete(`BinaryObjects/${this.deleteId}`).subscribe(
      () => {
        ref.close();
        this.httpService.success('Deleted Successfully');
        this.filterOrderBy
          ? this.pagination(
              this.page.pageNumber,
              this.page.pageSize,
              `${this.filterOrderBy}`
            )
          : this.pagination(this.page.pageNumber, this.page.pageSize);
      },
      (error) => {
        this.httpService.error(error.error.ServiceErrors[0]);
      }
    );
  }

  pageChanged(event): void {
    this.page.pageNumber = event;
    this.filterOrderBy
      ? this.pagination(event, this.page.pageSize, `${this.filterOrderBy}`)
      : this.pagination(event, this.page.pageSize);
  }

  pagination(pageNumber: number, pageSize: number, orderBy?: string): void {
    const top = pageSize;
    this.page.pageSize = pageSize;
    const skip = (pageNumber - 1) * pageSize;
    this.getAllBinaryObjects(top, skip, orderBy);
  }

  selectChange(event): void {
    if (event.target.value) {
      this.page.pageNumber = 1;
      this.page.pageSize = +event.target.value;
      event.target.value && this.filterOrderBy
        ? this.pagination(
            this.page.pageNumber,
            this.page.pageSize,
            `${this.filterOrderBy}`
          )
        : this.pagination(this.page.pageNumber, this.page.pageSize);
    }
  }

  onSortClick(event, param: string): void {
    let target = event.currentTarget,
      classList = target.classList;
    if (classList.contains('fa-chevron-up')) {
      classList.remove('fa-chevron-up');
      classList.add('fa-chevron-down');
      this.filterOrderBy = `${param}+asc`;
      this.pagination(this.page.pageNumber, this.page.pageSize, `${param}+asc`);
    } else {
      classList.remove('fa-chevron-down');
      classList.add('fa-chevron-up');
      this.filterOrderBy = `${param}+desc`;
      this.pagination(
        this.page.pageNumber,
        this.page.pageSize,
        `${param}+desc`
      );
    }
  }

  editFile(id: string): void {
    this.router.navigate([`/pages/file/edit/${id}`]);
  }
}
