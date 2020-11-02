import { Component, OnInit } from '@angular/core';
import { NbToastrService } from '@nebular/theme';
import { UsersTeamService } from '../users-team.service';
import { Page } from '../../../interfaces/paginateInstance';
import { HelperService } from '../../../@core/services/helper.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';

@Component({
  selector: 'ngx-requests-user',
  templateUrl: './requests-user.component.html',
  styleUrls: ['./requests-user.component.scss'],
})
export class RequestsUserComponent implements OnInit {
  showpage: any = [];
  page: Page = {};
  sortDir = 1;
  get_perPage: boolean = false;
  OrganizationID: string;
  isAdmin: string;
  admin_name: string;
  get_allpeople: any = [];
  pendingRequestList: any = [];
  lengthOfRequest: any;
  per_page_num: any = [];
  feild_name: any = [];
  show_perpage_size: boolean = false;
  itemsPerPage: ItemsPerPage[] = [];

  constructor(
    protected userteamService: UsersTeamService,
    private helperService: HelperService,
    private toastrService: NbToastrService
  ) {
    this.OrganizationID = localStorage.getItem('ActiveOrganizationID');
    this.isAdmin = localStorage.getItem('isAdministrator');
    this.admin_name = localStorage.getItem('ActiveOrgname');
  }

  ngOnInit(): void {
    this.page.pageNumber = 1;
    this.page.pageSize = 5;
    this.pagination(this.page.pageNumber, this.page.pageSize);
    this.itemsPerPage = this.helperService.getItemsPerPage();
  }

  onSortClick(event, fil_val) {
    let target = event.currentTarget,
      classList = target.classList;
    if (classList.contains('fa-chevron-up')) {
      classList.remove('fa-chevron-up');
      classList.add('fa-chevron-down');
      let sort_set = 'desc';
      this.sort(fil_val, sort_set);
      this.sortDir = -1;
    } else {
      classList.add('fa-chevron-up');
      classList.remove('fa-chevron-down');
      let sort_set = 'asc';
      this.sort(fil_val, sort_set);
      this.sortDir = 1;
    }
  }

  sort(filter_value, vale) {
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    this.feild_name = filter_value + '+' + vale;
    this.userteamService
      .getPeoplePendingOrder(
        this.OrganizationID,
        this.page.pageSize,
        skip,
        this.feild_name
      )
      .subscribe((data: any) => {
        this.get_allpeople = data.items;
        this.page.totalCount = data.totalCount;
      });
  }

  per_page(val) {
    if (this.feild_name.length == 0) {
      this.per_page_num = val;
      this.page.pageSize = val;
      this.show_perpage_size = true;
      const skip = (this.page.pageNumber - 1) * this.per_page_num;
      this.userteamService
        .getPeoplePending(this.OrganizationID, this.per_page_num, skip)
        .subscribe((data: any) => {
          this.get_allpeople = data.items;
          this.page.totalCount = data.totalCount;
        });
    } else if (this.feild_name.length != 0) {
      this.per_page_num = val;
      this.page.pageSize = val;
      this.show_perpage_size = true;
      const skip = (this.page.pageNumber - 1) * this.per_page_num;
      this.userteamService
        .getPeoplePendingOrder(
          this.OrganizationID,
          this.per_page_num,
          skip,
          this.feild_name
        )
        .subscribe((data: any) => {
          this.get_allpeople = data.items;
          this.page.totalCount = data.totalCount;
        });
    }
  }

  pageChanged(event) {
    this.page.pageNumber = event;
    this.pagination(event, this.page.pageSize);
  }

  pagination(pageNumber, pageSize) {
    if (this.feild_name.length == 0) {
      if (this.show_perpage_size == false) {
        const top: number = pageSize;
        const skip = (pageNumber - 1) * pageSize;
        this.getRequests(top, skip);
      } else if (this.show_perpage_size == true) {
        const top: number = this.page.pageSize;
        const skip = (pageNumber - 1) * this.page.pageSize;
        this.getRequests(top, skip);
      }
    } else if (this.feild_name.length != 0) {

      this.show_perpage_size = true;
      this.userteamService
        .getPeopleOrder(
          this.OrganizationID,
          this.page.pageSize,
          this.page.pageNumber,
          this.feild_name
        )
        .subscribe((data: any) => {
          this.get_allpeople = data.items;
          this.page.totalCount = data.totalCount;
        });
    }
  }

  /**
   * TODO gets ending approval lists for a specific organization
   * @param null
   * @returns void
   */
  getRequests(top, skip): void {
    this.get_perPage = false;
    this.userteamService
      .getPeoplePending(this.OrganizationID, top, skip)
      .subscribe((data: any) => {
        this.showpage = data;
        this.pendingRequestList = data.items;
        this.lengthOfRequest = data.items.length;
        this.page.totalCount = data.totalCount;
        this.get_perPage = true;
      });
  }

  /**
   * TODO Approve Request
   * @param personId: string
   * @returns void
   */
  ApproveRequest(personId: string): void {
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    const obj = {
      organizationId: this.OrganizationID,
      id: personId,
    };

    this.userteamService
      .ApproveMember(this.OrganizationID, personId, obj)
      .subscribe(() => {
        this.toastrService.success('You have successfully Approved');
        this.getRequests(this.page.pageSize, skip);
      });
  }

  /**
   * TODO Reject Access Request
   * @param personId: string
   * @returns void
   */
  DenyRequest(personId: string): void {
    const skip = (this.page.pageNumber - 1) * this.page.pageSize;
    const obj = {
      organizationId: this.OrganizationID,
      id: personId,
    };

    this.userteamService
      .RejectMember(this.OrganizationID, personId, obj)
      .subscribe(() => {
        this.toastrService.success('Request Rejected Successfully');
        this.getRequests(this.page.pageSize, skip);
      });
  }
}
