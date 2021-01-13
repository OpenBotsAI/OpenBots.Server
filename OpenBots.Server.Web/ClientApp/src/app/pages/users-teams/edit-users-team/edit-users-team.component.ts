
import { Component, OnInit, TemplateRef, Inject } from '@angular/core';
import { NbToastrService, NbDialogService } from '@nebular/theme';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EMAIL_PATTERN } from '../../../@auth/components/constants';
import { getDeepFromObject } from '../../../@auth/helpers';
import { NB_AUTH_OPTIONS } from '@nebular/auth';
import { UsersTeamService } from '../users-team.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'ngx-edit-users-team',
  templateUrl: './edit-users-team.component.html',
  styleUrls: ['./edit-users-team.component.scss']
})
export class EditUsersTeamComponent implements OnInit {
  memberId: any = [];
  personId: any = []
  memberName: any = [];
  memeberEmail: any = [];
  OrganizationID: string;
  memberRec: any = []
  isAdmin: string;
  admin_name: string;
  get_allpeople: any = []
  AmIAdmin: string;
  userinviteForm: FormGroup;
  submitted = false;
  view_dialog: any;
  toggle: boolean;
  checked = false;

  constructor(protected userteamService: UsersTeamService, private formBuilder: FormBuilder, protected router: Router,
    private toastrService: NbToastrService, private acroute: ActivatedRoute,) {
    this.acroute.queryParams.subscribe((params) => {
      this.personId = params.personId;
      // this.OrganizationID = params.orgid;
      // this.memberName = params.name;
      // this.memeberEmail = params.email;
 
      // this.getMemberDetail(params.id);
    });
  }

  ngOnInit(): void {
    this.userinviteForm = this.formBuilder.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.pattern("^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[a-z]{2,4}$")]],
      password: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      

    });
    this.getMemberDetail();

  }

  get f() { return this.userinviteForm.controls; }



  getMemberDetail() {
    this.userteamService
      .getMemberDetail(this.personId)
      .subscribe((data: any) => {
        console.log(data);
        this.memberRec = data;
        // this.userinviteForm.patchValue()
        this.userinviteForm.patchValue(data);
      });
  }


  check(checked: boolean) {
    this.checked = checked;
  }

  onSubmit() {
    this.submitted = true;
    if (this.userinviteForm.invalid) {
      return;
    }
    this.userteamService.updateMember(this.personId, this.userinviteForm.value).subscribe(
      () => {
        this.toastrService.success('Updated Successfully!', 'Success');
        this.userinviteForm.reset();
        this.router.navigate(['pages/users/teams-member'])
      }, () => this.submitted = false);
  }

  onReset() {
    this.submitted = false;
    this.userinviteForm.reset();
  }

}
