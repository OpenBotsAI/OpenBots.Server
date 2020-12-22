import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HelperService } from '../../../../@core/services/helper.service';
import { HttpService } from '../../../../@core/services/http.service';
import { Rule, Usage } from '../../../../interfaces/ipFencing';
import { RxwebValidators, IpVersion } from '@rxweb/reactive-form-validators';
import { IpFencingApiUrl } from '../../../../webApiUrls';
@Component({
  selector: 'ngx-add-rule',
  templateUrl: './add-rule.component.html',
  styleUrls: ['./add-rule.component.scss'],
})
export class AddRuleComponent implements OnInit {
  usage: Usage[];
  rule: Rule[];
  ruleForm: FormGroup;
  organizationId: string;
  editRuleId: string;
  title = 'Add';
  eTag: string;
  isSubmitted = false;

  constructor(
    private fb: FormBuilder,
    private httpService: HttpService,
    private router: Router,
    private route: ActivatedRoute,
    private helperService: HelperService
  ) {}

  ngOnInit(): void {
    this.usage = this.helperService.getUsage();
    this.rule = this.helperService.getRules();
    if (localStorage.getItem('ActiveOrganizationID'))
      this.organizationId = localStorage.getItem('ActiveOrganizationID');
    this.editRuleId = this.route.snapshot.params['id'];
    if (this.editRuleId) {
      this.getRuleById();
      this.title = 'Update';
    }
    this.ruleForm = this.initializeRuleForm();
  }

  initializeRuleForm() {
    return this.fb.group({
      usage: ['', [Validators.required]],
      rule: ['', [Validators.required]],
      ipAddress: [''],
      ipRange: [''],
      headerName: [''],
      headerValue: [''],
    });
  }

  get formControls() {
    return this.ruleForm.controls;
  }

  onSubmit(): void {
    this.isSubmitted = true;
    if (this.editRuleId) this.updateRule();
    else this.addRule();
  }

  updateRule(): void {
    const headers = this.helperService.getETagHeaders(this.eTag);
    this.httpService
      .put(
        `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.IPFencing}/${this.editRuleId}`,
        this.ruleForm.value,
        { observe: 'response', headers }
      )
      .subscribe(
        (response) => {
          if (response && response.status === 200) {
            this.httpService.success('Rule updated successfully');
            this.router.navigate(['/pages/config/settings']);
          }
        },
        () => (this.isSubmitted = false)
      );
  }

  addRule(): void {
    this.httpService
      .post(
        `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.IPFencing}`,
        this.ruleForm.value,
        { observe: 'response' }
      )
      .subscribe(
        (response) => {
          if (response && response.status === 201) {
            this.httpService.success('Rule created successfully');
            this.router.navigate(['/pages/config/settings']);
          }
        },
        () => (this.isSubmitted = false)
      );
  }

  onRuleChange(value) {
    if (value == 'IPv4' || value === 'IPv4') {
      this.ruleForm.get('ipAddress').clearValidators();
      this.ruleForm.get('ipAddress').reset();
      this.ruleForm
        .get('ipAddress')
        .setValidators([
          Validators.required,
          RxwebValidators.ip({ version: IpVersion.V4 }),
        ]);
      this.ruleForm.get('ipAddress').updateValueAndValidity();
    } else if (value == 'IPv6') {
      this.ruleForm.get('ipAddress').clearValidators();
      this.ruleForm.get('ipAddress').reset();
      this.ruleForm
        .get('ipAddress')
        .setValidators([
          Validators.required,
          RxwebValidators.ip({ version: IpVersion.V6 }),
        ]);
      this.ruleForm.get('ipAddress').updateValueAndValidity();
    } else if (value == 'IPv4Range') {
      this.ruleForm.get('ipRange').clearValidators();
      this.ruleForm.get('ipRange').reset();
      this.ruleForm
        .get('ipRange')
        .setValidators([
          Validators.required,
          RxwebValidators.ip({ version: IpVersion.V4, isCidr: true }),
        ]);
      this.ruleForm.get('ipRange').updateValueAndValidity();
    } else if (value == 'IPv6Range') {
      this.ruleForm.get('ipRange').clearValidators();
      this.ruleForm.get('ipRange').reset();
      this.ruleForm
        .get('ipRange')
        .setValidators([
          Validators.required,
          RxwebValidators.ip({ version: IpVersion.V6, isCidr: true }),
        ]);
      this.ruleForm.get('ipRange').updateValueAndValidity();
    }
  }

  getRuleById(): void {
    this.httpService
      .get(
        `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.IPFencing}/${this.editRuleId}`,
        { observe: 'response' }
      )
      .subscribe((response) => {
        if (response && response.status == 200) {
          this.eTag = response.headers.get('etag');
          for (let data of this.usage) {
            if (response.body.usage == data.value) {
              response.body.usage = data.name;
            }
          }
          for (let data of this.rule) {
            if (response.body.rule == data.value) {
              response.body.rule = data.name;
              this.onRuleChange(response.body.rule);
            }
          }
          this.ruleForm.patchValue({ ...response.body });
        }
      });
  }
}
