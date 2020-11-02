import { Component, OnInit } from '@angular/core';
import { NbToastrService } from '@nebular/theme';
import { UsersTeamService } from '../users-team.service';
import { Router } from '@angular/router';
import { Page } from '../../../interfaces/paginateInstance';
import { DialogService } from '../../../@core/dialogservices/dialog.service';
import { ItemsPerPage } from '../../../interfaces/itemsPerPage';
import { HelperService } from '../../../@core/services/helper.service';

@Component({
  selector: 'ngx-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss'],
})
export class UsersComponent implements OnInit {
  showpage: any = [];
  OrganizationID: string;
  sortDir = 1;
  isAdmin: string;
  admin_name: string;
  get_allpeople: any = [];
  AmIAdmin: string;
  submitted = false;
  view_dialog: any;
  toggle: boolean;
  page: Page = {};
  show_perpage_size: boolean = false;
  per_page_num: any = [];
  feild_name: any = [];
  orgMemberId: string;
  isDeleted = false;
  get_perPage: boolean = false;
  itemsPerPage: ItemsPerPage[] = [];

  constructor(
    protected userteamService: UsersTeamService,
    private toastrService: NbToastrService,
    protected router: Router,
    private helperService: HelperService,
    private dialogService: DialogService
  ) {
    this.OrganizationID = localStorage.getItem('ActiveOrganizationID');
    this.isAdmin = localStorage.getItem('isAdministrator');
    this.admin_name = localStorage.getItem('ActiveOrgname');
    this.AmIAdmin = localStorage.getItem('personId');
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
      .getPeopleOrder(
        this.OrganizationID,
        this.page.pageSize,
        skip,
        this.feild_name
      )
      .subscribe((data: any) => {
        this.showpage = data;
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
        .getPeople(this.OrganizationID, this.per_page_num, skip)
        .subscribe((data: any) => {
          this.showpage = data;
          this.get_allpeople = data.items;
          this.page.totalCount = data.totalCount;
        });
    } else if (this.feild_name.length != 0) {
      this.per_page_num = val;
      this.page.pageSize = val;
      this.show_perpage_size = true;
      const skip = (this.page.pageNumber - 1) * this.per_page_num;
      this.userteamService
        .getPeopleOrder(
          this.OrganizationID,
          this.per_page_num,
          skip,
          this.feild_name
        )
        .subscribe((data: any) => {
          this.showpage = data;
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
        this.getPeople(top, skip);
      } else if (this.show_perpage_size == true) {
        const top: number = this.page.pageSize;
        const skip = (pageNumber - 1) * this.page.pageSize;
        this.getPeople(top, skip);
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
          this.showpage = data;
          this.get_allpeople = data.items;
          this.page.totalCount = data.totalCount;
        });
    }
  }

  gotoadd() {
    this.router.navigate(['/pages/users/add-teams']);
  }

  deleteUser(ref) {
    this.isDeleted = true;
    this.userteamService
      .deleteMember(this.OrganizationID, this.orgMemberId)
      .subscribe(
        () => {
          ref.close();
          this.isDeleted = false;
          this.toastrService.success('You have successfully Delete ');
          this.pagination(this.page.pageNumber, this.page.pageSize);
        },
        (error) => {
          this.isDeleted = false;
          this.toastrService.danger(error.error.serviceErrors[0]);
        }
      );
  }

  getPeople(top, skip): void {
    this.get_perPage = false;
    this.userteamService
      .getPeople(this.OrganizationID, top, skip)
      .subscribe((data: any) => {
        this.showpage = data;
        this.get_allpeople = data.items;
        this.page.totalCount = data.totalCount;
        this.get_perPage = true;
      });
  }

  onChange(value, personId: string) {
    this.toggle = value.target.checked;
    this.adminRights(this.toggle, personId);
  }

  adminRights(admin: boolean, personId: string) {
    if (admin == true) {
      this.grantAdmin(personId);
    } else if (admin == false) {
      this.revokeAdmin(personId);
    }
  }

  grantAdmin(personId): void {
    this.userteamService
      .AllowgrantAdmin(personId, this.OrganizationID)
      .subscribe(() => {
        this.toastrService.success('Admin Rights Granted');
        this.pagination(this.page.pageNumber, this.page.pageSize);
      });
  }

  revokeAdmin(personId): void {
    this.userteamService
      .RemovegrantAdmin(personId, this.OrganizationID)
      .subscribe(() => {
        this.toastrService.success('Admin Rights Revoked');
        this.pagination(this.page.pageNumber, this.page.pageSize);
      });
  }

  openDialog(ref, id) {
    this.orgMemberId = id;
    this.dialogService.openDialog(ref);
  }
}
