import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { NB_AUTH_OPTIONS, NbAuthService } from '@nebular/auth';
import { getDeepFromObject } from '../../helpers';
import { HttpService } from '../../../@core/services/http.service';
import { NbToastrService } from '@nebular/theme';

@Component({
  selector: 'ngx-reset-forget-password',
  templateUrl: './reset-forget-password.component.html',
  styleUrls: ['./reset-forget-password.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResetForgetPasswordComponent implements OnInit {

  minLength: number = this.getConfigValue('forms.validation.password.minLength');
  maxLength: number = this.getConfigValue('forms.validation.password.maxLength');
  redirectDelay: number = this.getConfigValue('forms.resetPassword.redirectDelay');
  showMessages: any = this.getConfigValue('forms.resetPassword.showMessages');
  strategy: string = this.getConfigValue('forms.resetPassword.strategy');
  isPasswordRequired: boolean = this.getConfigValue('forms.validation.password.required');

  submitted = false;
  errors: string[] = [];
  messages: string[] = [];
  user: any = {};
  resetPasswordForm: FormGroup;
  resetForgotPasswordUserId;
  resetForgotPasswordToken;
  constructor(protected service: NbAuthService,
    @Inject(NB_AUTH_OPTIONS) protected options = {},
    protected cd: ChangeDetectorRef, private toastrService: NbToastrService,
    protected fb: FormBuilder, private httpService: HttpService, private route: ActivatedRoute,
    protected router: Router) { 
      this.route.queryParams.subscribe(params => {
        this.resetForgotPasswordUserId = params.userid,
          this.resetForgotPasswordToken = encodeURIComponent(params.token);
         
      });
    }

  ngOnInit(): void {
    const passwordValidators = [
      Validators.minLength(this.minLength),
      Validators.maxLength(this.maxLength),
    ];
    this.isPasswordRequired && passwordValidators.push(Validators.required);

    this.resetPasswordForm = this.fb.group({
     
      password: this.fb.control('', [...passwordValidators]),
      confirmPassword: this.fb.control('', [...passwordValidators]),
    });
  }
 
  get password() { return this.resetPasswordForm.get('password'); }
  get confirmPassword() { return this.resetPasswordForm.get('confirmPassword'); }

  resetPass(): void {
    
    this.errors = this.messages = [];
    this.submitted = true;
    this.user = this.resetPasswordForm.value;   
    var formData = {
      newPassword: this.resetPasswordForm.value.password,
      userId: this.resetForgotPasswordUserId,
      token:  this.resetForgotPasswordToken,
    };
    this.httpService.put('Auth/SetPassword', formData).subscribe((result) => {
      this.toastrService.success('Your password has been updated!', 'Success');
      this.router.navigate(['auth/login'])      
    });
  }

  getConfigValue(key: string): any {
    return getDeepFromObject(this.options, key, null);
  }
}
