
import { Component, OnInit, TemplateRef, Inject } from '@angular/core';
import { NbToastrService, NbDialogService } from '@nebular/theme';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EMAIL_PATTERN } from '../../../@auth/components/constants';
import { getDeepFromObject } from '../../../@auth/helpers';
import { NB_AUTH_OPTIONS} from '@nebular/auth';
import { UsersTeamService } from '../users-team.service';
import { Router } from '@angular/router';

@Component({
  selector: 'ngx-add-users-team',
  templateUrl: './add-users-team.component.html',
  styleUrls: ['./add-users-team.component.scss']
})
export class AddUsersTeamComponent implements OnInit {
 
  OrganizationID: string;
  isAdmin: string;
  admin_name :string;
  get_allpeople : any =[]
  AmIAdmin: string;
  userinviteForm: FormGroup;
  submitted = false;
  view_dialog:any;
  toggle :boolean;
  checked = false;

  constructor(protected userteamService:UsersTeamService, private formBuilder: FormBuilder, protected router :Router,
    private toastrService: NbToastrService) { }


  
  ngOnInit(): void {
    this.userinviteForm = this.formBuilder.group({
      name: [ '',[Validators.required ,Validators.minLength(3), Validators.maxLength(100)]],
      email: [ '',[Validators.required ,Validators.pattern("^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[a-z]{2,4}$")]],
      password: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      skipEmailVerification: [this.checked]
  
  });
  }

    get f() { return this.userinviteForm.controls; }


      

  gotoadd() {
    this.router.navigate(['/pages/users/add-teams'])
  }
 
 
  check(checked: boolean) {
    this.checked = checked;
  }

  onSubmit() {
    this.submitted = true;    
    if (this.userinviteForm.invalid) {
        return;
    }
  this.userteamService.inviteMember(this.userinviteForm.value).subscribe(
      () => {
        this.toastrService.success('Invitation Sent  Successfully! ');
        this.userinviteForm.reset();
        this.router.navigate(['pages/users/teams-member'])
    }, () => this.submitted = false);       
}

onReset() {
    this.submitted = false;
    this.userinviteForm.reset();
}
 
}
