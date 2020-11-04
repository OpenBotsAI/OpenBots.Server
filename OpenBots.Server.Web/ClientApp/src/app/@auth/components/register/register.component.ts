import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  Inject,
  OnInit,
} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import {
  NB_AUTH_OPTIONS,
  NbAuthSocialLink,
  NbAuthService,
} from '@nebular/auth';
import { getDeepFromObject } from '../../helpers';
import { EMAIL_PATTERN } from '../constants';
import { HttpService } from '../../../@core/services/http.service';
import { NbToastrService } from '@nebular/theme';
import { HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'ngx-register',
  styleUrls: ['./register.component.scss'],
  templateUrl: './register.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NgxRegisterComponent implements OnInit {
  minLoginLength: number = this.getConfigValue(
    'forms.validation.fullName.minLength'
  );
  maxLoginLength: number = this.getConfigValue(
    'forms.validation.fullName.maxLength'
  );
  minLength: number = this.getConfigValue(
    'forms.validation.password.minLength'
  );
  maxLength: number = this.getConfigValue(
    'forms.validation.password.maxLength'
  );
  isFullNameRequired: boolean = this.getConfigValue(
    'forms.validation.fullName.required'
  );
  isEmailRequired: boolean = this.getConfigValue(
    'forms.validation.email.required'
  );
  isDptRequired: boolean = this.getConfigValue(
    'forms.validation.dpt_name.required'
  );
  isOrgRequired: boolean = this.getConfigValue('forms.validation.Organization');
  isPasswordRequired: boolean = this.getConfigValue(
    'forms.validation.password.required'
  );
  redirectDelay: number = this.getConfigValue('forms.register.redirectDelay');
  showMessages: any = this.getConfigValue('forms.register.showMessages');
  strategy: string = this.getConfigValue('forms.register.strategy');
  socialLinks: NbAuthSocialLink[] = this.getConfigValue(
    'forms.login.socialLinks'
  );

  submitted = false;
  errors: string[] = [];
  messages: string[] = [];
  user: any = {};
  totalOrgCount: number;
  registerForm: FormGroup;
  CreateNeworganization: boolean;
  constructor(
    protected service: NbAuthService,
    @Inject(NB_AUTH_OPTIONS) protected options = {},
    protected cd: ChangeDetectorRef,
    private fb: FormBuilder,
    private httpService: HttpService,
    protected router: Router,
    private toastrService: NbToastrService
  ) {
    this.httpService
      .get('Organizations/TotalOrganizationCount')
      .subscribe((result) => {
        this.totalOrgCount = result;
      });
  }

  get login() {
    return this.registerForm.get('fullName');
  }
  get email() {
    return this.registerForm.get('email');
  }
  get dpt_name() {
    return this.registerForm.get('dpt_name');
  }
  get Organization() {
    return this.registerForm.get('Organization');
  }
  get password() {
    return this.registerForm.get('password');
  }
  get confirmPassword() {
    return this.registerForm.get('confirmPassword');
  }

  ngOnInit(): void {
    const loginValidators = [
      Validators.minLength(2),
      Validators.maxLength(100),
    ];
    this.isFullNameRequired && loginValidators.push(Validators.required);

    const emailValidators = [Validators.pattern(EMAIL_PATTERN)];
    this.isEmailRequired && emailValidators.push(Validators.required);

    const dptValidators = [Validators.minLength(2), Validators.maxLength(100)];
    this.isDptRequired && dptValidators.push(Validators.required);

    const passwordValidators = [
      Validators.minLength(this.minLength),
      Validators.maxLength(this.maxLength),
    ];
    this.isPasswordRequired && passwordValidators.push(Validators.required);
    if (this.totalOrgCount === 0) {
      this.CreateNeworganization = true;
      const orgValidators = [
        Validators.minLength(2),
        Validators.maxLength(100),
      ];
      this.isOrgRequired && orgValidators.push(Validators.required);
      this.registerForm = this.fb.group({
        fullName: this.fb.control('', [...loginValidators]),
        email: this.fb.control('', [...emailValidators]),
        dpt_name: this.fb.control('', [...dptValidators]),
        Organization: this.fb.control('', [...orgValidators]),
        password: this.fb.control('', [...passwordValidators]),
        confirmPassword: this.fb.control('', [...passwordValidators]),
      });
    } else {
      this.registerForm = this.fb.group({
        fullName: this.fb.control('', [...loginValidators]),
        email: this.fb.control('', [...emailValidators]),
        dpt_name: this.fb.control('', [...dptValidators]),
        Organization: this.fb.control(''),
        password: this.fb.control('', [...passwordValidators]),
        confirmPassword: this.fb.control('', [...passwordValidators]),
      });
      this.CreateNeworganization = false;
    }
  }

  register(): void {
    const headers = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
    };
    this.user = this.registerForm.value;
    this.errors = this.messages = [];
    this.submitted = true;
    const url = 'Auth/Register';
    if (this.totalOrgCount === 0) {
      this.CreateNeworganization = true;
    }
    const RegCredentials = {
      Name: this.registerForm.value.fullName,
      Email: this.registerForm.value.email,
      Organization: this.registerForm.value.Organization,
      Department: this.registerForm.value.dpt_name,
      CreateNeworganization: this.CreateNeworganization,
      Password: this.registerForm.value.password,
    };

    this.httpService.post(url, RegCredentials, headers).subscribe((response) => {
    this.submitted = false;

      if (response) {
        this.httpService.success('You have registered successfully');
    }
      this.router.navigate(['auth/login']);
      this.registerForm.reset();
    });
    this.submitted = false;
  }

  getConfigValue(key: string): any {
    return getDeepFromObject(this.options, key, null);
  }
}
