import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NbToastrService } from '@nebular/theme';
import { AgentsService } from '../agents.service';
import { Router } from '@angular/router';

@Component({
  selector: 'ngx-add-agents',
  templateUrl: './add-agents.component.html',
  styleUrls: ['./add-agents.component.scss'],
})
export class AddAgentsComponent implements OnInit {
  addagent: FormGroup;
  submitted = false;
  cred_value: any = [];
  value = ['JSON', 'Number', 'Text'];
  constructor(
    private formBuilder: FormBuilder,
    protected agentService: AgentsService,
    protected router: Router,
    private toastrService: NbToastrService
  ) {}

  ngOnInit(): void {
    this.addagent = this.formBuilder.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
          Validators.pattern(
            '^[A-Za-z0-9_.-]{3,100}$'
          ),
        ],
      ],
      machineName: [
        '',
        [
          Validators.required,
          Validators.pattern(
            /^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])?(.\\)?(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$/
          ),
        ],
      ],

      macAddresses: [''],
      ipAddresses: [
        '',
        Validators.pattern(
          '^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$'
        ),
      ],
      isEnabled: [true],
      CredentialId: ['', Validators.required],
      userName: ['', Validators.required],
      password: ['', Validators.required],
    });

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

  onSubmit() {
    this.submitted = true;
    if (this.addagent.invalid) {
      return;
    }
    this.agentService.addAgent(this.addagent.value).subscribe((data) => {
      this.toastrService.success('Add Agent Successfully!', 'Success');
      this.router.navigate(['pages/agents/list']);
    }, () => this.submitted = false);

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
}
