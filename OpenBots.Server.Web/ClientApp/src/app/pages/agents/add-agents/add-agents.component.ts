import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NbToastrService } from '@nebular/theme';
import { AgentsService } from '../agents.service';
import { ActivatedRoute, Router } from '@angular/router';
import { IpVersion, RxwebValidators } from '@rxweb/reactive-form-validators';
import { HttpResponse } from '@angular/common/http';

@Component({
  selector: 'ngx-add-agents',
  templateUrl: './add-agents.component.html',
  styleUrls: ['./add-agents.component.scss'],
})
export class AddAgentsComponent implements OnInit {
  addagent: FormGroup;
  etag;
  title = 'Add';
  checked = false;
  submitted = false;
  cred_value: any = [];
  value = ['JSON', 'Number', 'Text'];
  ipVersion = 'V4';
  urlId: string;
  show_allagents: any = [];
  constructor(
    private formBuilder: FormBuilder,
    private agentService: AgentsService,
    private router: Router,
    private route: ActivatedRoute,
    private toastrService: NbToastrService
  ) {
    this.urlId = this.route.snapshot.params['id'];
  }

  ngOnInit(): void {
    this.addagent = this.formBuilder.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
          Validators.pattern('^[A-Za-z0-9_.-]{3,100}$'),
        ],
      ],
      machineName: [''],
      macAddresses: [''],
      ipAddresses: ['', [RxwebValidators.ip({ version: IpVersion.V4 })]],
      isEnabled: [true],
      CredentialId: ['', [Validators.required]],
      userName: ['', [Validators.required]],
      password: ['', [Validators.required]],
      ipOption: ['ipv4'],
      isEnhancedSecurity: false,
    });
  // this.addagent = this.formBuilder.group({
  //   name: [
  //     '',
  //     [
  //       Validators.required,
  //       Validators.minLength(3),
  //       Validators.maxLength(100),
  //       Validators.pattern('^[A-Za-z0-9_.-]{3,100}$'),
  //     ],
  //   ],
  //   machineName: ['', [Validators.required]],
  //   macAddresses: [''],
  //   ipAddresses: [''],
  //   isEnabled: [''],
  //   CredentialId: ['', [Validators.required]],
  //   ipOption: [''],
  //   isEnhancedSecurity: false,
  // });





    if (this.urlId) {
      this.getagentID(this.urlId);
      //  this.submitted = true;
      // this.addagent.get('macAddresses').clearValidators();
      // this.addagent.get('macAddresses').updateValueAndValidity();
      // this.addagent.get('ipAddresses').clearValidators();
      // this.addagent.get('ipAddresses').updateValueAndValidity();
      // this.addagent.get('ipOption').clearValidators();
      // this.addagent.get('ipOption').updateValueAndValidity();
      this.addagent.get('password').clearValidators();
      this.addagent.get('password').updateValueAndValidity();
      // this.addagent.get('isEnabled').clearValidators();
      // this.addagent.get('isEnabled').updateValueAndValidity();

      this.title = 'Update';
    }

    this.get_cred();
  }
  get_cred() {
    this.agentService.getCred().subscribe((data: any) => {
      this.cred_value = data;
    });
  }
  get f() {
    return this.addagent.controls;
  }
  check(checked: boolean) {
    this.checked = checked;
    if (checked) {
      this.addagent
        .get('macAddresses')
        .setValidators([
          Validators.required,
          Validators.pattern('^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$'),
        ]);
      this.addagent.get('ipAddresses').setValidators([Validators.required]);
      this.addagent.get('macAddresses').updateValueAndValidity();
      this.addagent.get('ipAddresses').updateValueAndValidity();
    } else {
      this.addagent.get('ipAddresses').clearValidators();
      this.addagent.get('ipAddresses').updateValueAndValidity();
      this.addagent.get('macAddresses').clearValidators();
      this.addagent.get('macAddresses').updateValueAndValidity();
    }
  }

  onSubmit() {
    if (this.urlId) {
      this.UpdateAgent();
    } else {
      this.AddAgent();
    }
  }

  AddAgent() {
    this.submitted = true;
    if (this.addagent.invalid) {
      return;
    }
    this.agentService.addAgent(this.addagent.value).subscribe(
      () => {
        this.toastrService.success('Agent added successfully', 'Success');
        this.router.navigate(['pages/agents/list']);
      },
      () => {
        this.submitted = false;
      }
    );
  }

  UpdateAgent() {
    this.submitted = true;
    this.agentService
      .editAgent(this.urlId, this.addagent.value, this.etag)
      .subscribe(
        () => {
          this.toastrService.success('Updated successfully', 'Success');
          this.router.navigate(['pages/agents/list']);
        },
        (error) => {
          if (error.error.status === 409) {
            this.toastrService.danger(error.error.serviceErrors, 'error');
            this.getagentID(this.urlId);
            this.submitted = false;
          }
          if (error.error.status === 429) {
            this.toastrService.danger(error.error.serviceErrors, 'error');
            // this.get_allagent(this.agent_id)
            this.submitted = false;
          }
        }
      );
  }

  onReset() {
    this.submitted = false;
    this.addagent.reset();
  }

  keyPressAlphaNumericWithCharacters(event) {
    var inp = String.fromCharCode(event.keyCode);
    if (/[a-zA-Z0-9-/. ]/.test(inp)) {
      return true;
    } else {
      event.preventDefault();
      return false;
    }
  }

  handleInput(event) {
    var key = event.keyCode;
    if (key === 32) {
      event.preventDefault();
      return false;
    }
  }

  radioSetValidator(value: string): void {
    this.addagent.get('ipAddresses').clearValidators();
    this.addagent.get('ipAddresses').reset();
    if (value === 'ipv4') {
      this.ipVersion = 'V4';
      this.addagent
        .get('ipAddresses')
        .setValidators([
          Validators.required,
          RxwebValidators.ip({ version: IpVersion.V4 }),
        ]);
      this.addagent.get('ipAddresses').updateValueAndValidity();
    } else {
      this.ipVersion = 'V6';
      this.addagent
        .get('ipAddresses')
        .setValidators([
          Validators.required,
          RxwebValidators.ip({ version: IpVersion.V6 }),
        ]);
      this.addagent.get('ipAddresses').updateValueAndValidity();
    }
  }
 
  getagentID(id) {
    this.agentService.getAgentbyID(id).subscribe((data: HttpResponse<any>) => {
      if (data && data.body) {
        this.show_allagents = data.body;
        if (data.body.ipOption === 'ipv6') {
          this.addagent
            .get('ipAddresses')
            .setValidators([
              Validators.required,
              RxwebValidators.ip({ version: IpVersion.V6 }),
            ]);
          this.addagent.get('ipAddresses').updateValueAndValidity();
          this.ipVersion = 'V6';
        } else {
          this.addagent
            .get('ipAddresses')
            .setValidators([
              Validators.required,
              RxwebValidators.ip({ version: IpVersion.V4 }),
            ]);
          this.addagent.get('ipAddresses').updateValueAndValidity();
        }
        this.etag = data.headers.get('ETag').replace(/\"/g, '');
        this.addagent.patchValue(this.show_allagents);
        this.addagent.patchValue({
          CredentialId: this.show_allagents.credentialId,
        });
      }
    });
  }

  
}
