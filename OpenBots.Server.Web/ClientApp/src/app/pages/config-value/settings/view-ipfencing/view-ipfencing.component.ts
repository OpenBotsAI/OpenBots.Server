import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { HelperService } from '../../../../@core/services/helper.service';
import { HttpService } from '../../../../@core/services/http.service';
import { Rule, Usage } from '../../../../interfaces/ipFencing';
import { IpFencingApiUrl } from '../../../../webApiUrls';

@Component({
  selector: 'ngx-view-ipfencing',
  templateUrl: './view-ipfencing.component.html',
  styleUrls: ['./view-ipfencing.component.scss'],
})
export class ViewIPFencingComponent implements OnInit {
  viewRuleId: string;
  organizationId: string;
  ruleForm: FormGroup;
  usage: Usage[];
  rule: Rule[];

  constructor(
    private httpService: HttpService,
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private helperService: HelperService
  ) {}

  ngOnInit(): void {
    this.usage = this.helperService.getUsage();
    this.rule = this.helperService.getRules();
    if (localStorage.getItem('ActiveOrganizationID'))
      this.organizationId = localStorage.getItem('ActiveOrganizationID');
    this.viewRuleId = this.route.snapshot.params['id'];
    this.ruleForm = this.initializeRuleForm();
    if (this.viewRuleId) this.getRuleByid();
  }

  initializeRuleForm() {
    return this.fb.group({
      usage: [''],
      rule: [''],
      ipAddress: [''],
      ipRange: [''],
      headerName: [''],
      headerValue: [''],
      createdBy: [''],
      createdOn: [''],
      updatedBy: [''],
      updatedOn: [''],
    });
  }

  getRuleByid(): void {
    this.httpService
      .get(
        `${IpFencingApiUrl.organizations}/${this.organizationId}/${IpFencingApiUrl.IPFencing}/${this.viewRuleId}`,
        { observe: 'response' }
      )
      .subscribe((response) => {
        if (response && response.status == 200) {
          for (let data of this.usage) {
            if (response.body.usage == data.value) {
              response.body.usage = data.name;
            }
          }
          for (let data of this.rule) {
            if (response.body.rule == data.value) {
              response.body.rule = data.name;
            }
          }
          response.body.createdOn = this.helperService.transformDate(
            response.body.createdOn,
            'll'
          );
          response.body.updatedOn = this.helperService.transformDate(
            response.body.updatedOn,
            'll'
          );
          this.ruleForm.patchValue({ ...response.body });
          this.ruleForm.disable();
        }
      });
  }
}
