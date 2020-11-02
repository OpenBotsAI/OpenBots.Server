import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { HttpService } from '../../../@core/services/http.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Name_Regex } from '../../../@auth/components';

@Component({
  selector: 'ngx-queue',
  templateUrl: './queue.component.html',
  styleUrls: ['./queue.component.scss'],
})
export class AddQueueComponent implements OnInit {
  queueForm: FormGroup;
  isSubmitted = false;
  urlId: string;
  title = 'Add';
  constructor(
    private fb: FormBuilder,
    private httpService: HttpService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.urlId = this.route.snapshot.params['id'];
    this.queueForm = this.initializeQueueForm();
    if (this.urlId) {
      this.getQueueById();
      this.title = 'Update';
    }
  }

  get formControls() {
    return this.queueForm.controls;
  }
  initializeQueueForm() {
    return this.fb.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
          Validators.pattern(Name_Regex),
        ],
      ],
      maxRetryCount: [0, [Validators.pattern(/^-?([0-9]|[0-9][0-9]d*)?$/)]],
      description: [''],
    });
  }

  onSubmit() {
    this.isSubmitted = true;
    if (this.urlId) this.updateQueue();
    else this.addQueue();
  }

  addQueue(): void {
    this.httpService
      .post('Queues', this.queueForm.value, { observe: 'response' })
      .subscribe(
        (response) => {
          if (response && response.status == 201) {
            this.isSubmitted = false;
            this.httpService.success('New Queue has been created');
            this.router.navigate(['pages/queueslist']);
            this.queueForm.reset();
          }
        },
        () => (this.isSubmitted = false)
      );
  }

  updateQueue() {
    this.httpService
      .put(`Queues/${this.urlId}`, this.queueForm.value, {
        observe: 'response',
      })
      .subscribe(
        (response) => {
          if (response && response.status == 200) {
            this.isSubmitted = false;
            this.httpService.success('Queue updated successfully');
            this.router.navigate(['pages/queueslist']);
            this.queueForm.reset();
          }
        },
        () => (this.isSubmitted = false)
      );
  }

  getQueueById(): void {
    this.httpService.get(`Queues/${this.urlId}`).subscribe((response) => {
      if (response) this.queueForm.patchValue({ ...response });
    });
  }
}
