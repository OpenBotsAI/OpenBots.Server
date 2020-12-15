import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { DialogService } from '../../../../@core/dialogservices';
import { HelperService } from '../../../../@core/services/helper.service';
import { HttpService } from '../../../../@core/services/http.service';
import { IPFencing, Rule, Usage } from '../../../../interfaces/ipFencing';
import { ItemsPerPage } from '../../../../interfaces/itemsPerPage';
import { Page } from '../../../../interfaces/paginateInstance';
import { IpFencingApiUrl } from '../ipFencing';

@Component({
  selector: 'ngx-all-settings',
  templateUrl: './all-settings.component.html',
  styleUrls: ['./all-settings.component.scss'],
})
export class AllSettingsComponent implements OnInit {
  organizationId: string;
  orgSettingId: string;
  IPFencingData: IPFencing[] = [];
  isChecked: boolean;
  ipFencingForm: FormGroup;
  filterOrderBy: string;
  page: Page = {};
  itemsPerPage: ItemsPerPage[] = [];
  deleteId: string;
  isDeleted = false;
  usage: Usage[];

  rule: Rule[];
  constructor(
    private router: Router,
    private httpService: HttpService,
    private fb: FormBuilder,
    private helperService: HelperService,
    private dialogService: DialogService
  ) {}

  ngOnInit(): void {
    this.usage = this.helperService.getUsage();
    this.rule = this.helperService.getRules();
    if (localStorage.getItem('ActiveOrganizationID'))
      this.organizationId = localStorage.getItem('ActiveOrganizationID');
    this.getSettingsId();
    this.itemsPerPage = this.helperService.getItemsPerPage();
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.ipFencingForm = this.fb.group({
      ipFencingMode: [],
    });
  }

  getSettingsId(): void {
    this.httpService
      .get(
        `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.organizationSettings}`
      )
      .subscribe((response) => {
        if (response && response.items.length)
          this.orgSettingId = response.items[0].id;
        this.getToggleButtonState();
        this.pagination(this.page.pageNumber, this.page.pageSize);
      });
  }

  pagination(pageNumber: number, pageSize: number, orderBy?: string): void {
    const top = pageSize;
    this.page.pageSize = pageSize;
    const skip = (pageNumber - 1) * pageSize;
    this.getAllIPFencing(top, skip, orderBy);
  }

  getAllIPFencing(top: number, skip: number, orderBy?: string): void {
    let url: string;
    if (orderBy)
      url = `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.IPFencing}?$orderby=${orderBy}&$top=${top}&$skip=${skip}`;
    else
      url = `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.IPFencing}?$orderby=createdOn+desc&$top=${top}&$skip=${skip}`;
    this.httpService.get(url).subscribe((response) => {
      if (response && response.items.length) {
        for (let items of response.items) {
          for (let data of this.usage) {
            if (items.usage == data.value) {
              items.usage = data.name;
            }
          }
          for (let data of this.rule) {
            if (items.rule == data.value) {
              items.rule = data.name;
            }
          }
        }
        this.IPFencingData = response.items;
        this.page.totalCount = response.totalCount;
      }
    });
  }
  addRule(): void {
    this.router.navigate(['pages/config/settings/rule/add']);
  }

  onToggleSecurityModel(event): void {
    this.isChecked = event.target.checked;
    let data: number;
    if (event.target.checked) data = 1;
    else data = -1;
    const arr = [
      {
        value: data,
        path: '/ipFencingMode',
        op: 'replace',
      },
    ];
    this.httpService
      .patch(
        `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.organizationSettings}/${this.orgSettingId}`,
        arr,
        { observe: 'response' }
      )
      .subscribe();
  }

  getToggleButtonState(): void {
    this.httpService
      .get(
        `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.organizationSettings}/${this.orgSettingId}`
      )
      .subscribe((response) => {
        if (response && response.ipFencingMode) {
          if (response.ipFencingMode == 1) {
            this.isChecked = true;
            this.ipFencingForm.patchValue({
              ipFencingMode: true,
            });
          } else {
            this.isChecked = false;
            this.ipFencingForm.patchValue({
              ipFencingMode: false,
            });
          }
        }
      });
  }
  trackByFn(index: number, item: unknown): number | null {
    if (!item) return null;
    return index;
  }

  pageChanged(event): void {
    this.page.pageNumber = event;
    if (this.filterOrderBy)
      this.pagination(event, this.page.pageSize, `${this.filterOrderBy}`);
    else this.pagination(event, this.page.pageSize);
  }

  selectChange(event): void {
    this.page.pageSize = +event.target.value;
    this.page.pageNumber = 1;
    if (event.target.value && this.filterOrderBy) {
      this.pagination(
        this.page.pageNumber,
        this.page.pageSize,
        `${this.filterOrderBy}`
      );
    } else this.pagination(this.page.pageNumber, this.page.pageSize);
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

  viewIPFencing(id: string): void {
    this.router.navigate([`/pages/config/settings/rule/view/${id}`]);
  }

  editIpFencing(id: string): void {
    this.router.navigate([`/pages/config/settings/rule/edit/${id}`]);
  }

  openDialog(ref, id: string): void {
    this.deleteId = id;
    this.dialogService.openDialog(ref);
  }

  deleteIPFencing(ref) {
    this.isDeleted = true;
    this.httpService
      .delete(
        `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.IPFencing}/${this.deleteId}`
      )
      .subscribe(
        () => {
          ref.close();
          this.httpService.success('Deleted Successfully');
          this.isDeleted = false;
          if (this.filterOrderBy) {
            this.pagination(
              this.page.pageNumber,
              this.page.pageSize,
              `${this.filterOrderBy}`
            );
          } else this.pagination(this.page.pageNumber, this.page.pageSize);
        },
        () => (this.isDeleted = false)
      );
  }
}
