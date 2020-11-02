import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import {
  NB_AUTH_OPTIONS,
  NbAuthSocialLink,
  NbAuthService,
} from '@nebular/auth';
import { getDeepFromObject } from '../../helpers';
import { NbThemeService, NbToastrService } from '@nebular/theme';
import { EMAIL_PATTERN } from '../constants';
import { HttpService } from '../../../@core/services/http.service';
import { HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'ngx-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})

export class NgxLoginComponent implements OnInit {

  minLength: number = this.getConfigValue('forms.validation.password.minLength');
  maxLength: number = this.getConfigValue('forms.validation.password.maxLength');
  redirectDelay: number = this.getConfigValue('forms.login.redirectDelay');
  showMessages: any = this.getConfigValue('forms.login.showMessages');
  strategy: string = this.getConfigValue('forms.login.strategy');
  socialLinks: NbAuthSocialLink[] = this.getConfigValue('forms.login.socialLinks');
  rememberMe = this.getConfigValue('forms.login.rememberMe');
  isEmailRequired: boolean = this.getConfigValue('forms.validation.email.required');
  isPasswordRequired: boolean = this.getConfigValue('forms.validation.password.required');

  errors: string[] = [];
  messages: string[] = [];
  user: any = {};
  submitted: boolean = false;
  loginForm: FormGroup;
  alive: boolean = true;
  showHealthStatus :boolean = false;
  get email() { return this.loginForm.get('email'); }
  get password() { return this.loginForm.get('password'); }

  constructor(protected service: NbAuthService,
    @Inject(NB_AUTH_OPTIONS) protected options = {},
    protected cd: ChangeDetectorRef,
    protected themeService: NbThemeService,
    private fb: FormBuilder,
    protected router: Router,
    private httpService: HttpService,
    private toastrService: NbToastrService) {}

  ngOnInit(): void {
    this.getHealthStatus();
    const emailValidators = [
      Validators.pattern(EMAIL_PATTERN),
    ];
    this.isEmailRequired && emailValidators.push(Validators.required);
    const passwordValidators = [
      Validators.minLength(this.minLength),
      Validators.maxLength(this.maxLength),
    ];
    this.isPasswordRequired && passwordValidators.push(Validators.required);

    this.loginForm = this.fb.group({
      email: this.fb.control('', [...emailValidators]),
      password: this.fb.control('', [...passwordValidators])
    });
  }


  getHealthStatus(){
    this.httpService.getHealthStatus('healthcheck-api').subscribe((data) => {
    })
  }
  login(): void {
    this.user = this.loginForm.value;
    this.errors = [];
    this.messages = [];
    this.submitted = true;
    const loginCredentials = {
      username: this.loginForm.value.email,
      password: this.loginForm.value.password
    }
    const headers = { headers: new HttpHeaders({ "Content-Type": "application/json" }) };

    this.httpService.post('Auth/token', loginCredentials, headers).subscribe((result) => {    
      if( result.isJoinOrgRequestPending == false &&  result.myOrganizations.length  !=0 ){
        this.submitted = false;
        this.SetLocalStorageForUser(result);
        this.toastrService.success('You have successfully logged in','Success');
        this.router.navigate(['pages/dashboard']);
        this.loginForm.reset();
      }
      else {
        this.submitted = false;
        this.toastrService.danger('Your request is not approved!','Failed');
      }
       
    })
    this.submitted = false;
  }

  getConfigValue(key: string): any {
    return getDeepFromObject(this.options, key, null);
  }


  SetLocalStorageForUser(data): void {
    localStorage.setItem('accessToken', data.token);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('isUserConsentRequired',JSON.stringify(data.isUserConsentRequired));
    localStorage.setItem('isPasswordSet', JSON.stringify(data.forcedPasswordChange) );
    localStorage.setItem('IsJoinOrgRequestPending',JSON.stringify(data.isJoinOrgRequestPending));
    localStorage.setItem('UserInfo', JSON.stringify(data));
    localStorage.setItem('OrganizationListLength', JSON.stringify(data.myOrganizations.length));
    if (data.myOrganizations.length !== 0) {
      localStorage.setItem(
        'ActiveOrganizationID',
        data.myOrganizations[0].id
      );
      localStorage.setItem(
        'ActiveOrgname',
        data.myOrganizations[0].name
      );
      localStorage.setItem(
        'isAdministrator',
        data.myOrganizations[0].isAdministrator
      );
    }
    localStorage.setItem('UserEmail', data.email);
    localStorage.setItem('personId', data.personId);
  }
}
