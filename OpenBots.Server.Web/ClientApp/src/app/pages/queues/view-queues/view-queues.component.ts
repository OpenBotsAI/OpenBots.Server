import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { HelperService } from '../../../@core/services/helper.service';
import { HttpService } from '../../../@core/services/http.service';

@Component({
  selector: 'ngx-view-queues',
  templateUrl: './view-queues.component.html',
  styleUrls: ['./view-queues.component.scss'],
})
export class ViewQueuesComponent implements OnInit {
  viewQueueForm: FormGroup;
  urlId: string;
  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private httpService: HttpService,
    private helperService: HelperService
  ) {}

  ngOnInit(): void {
    this.urlId = this.route.snapshot.params['id'];
    this.viewQueueForm = this.initializeQueueForm();
    if (this.urlId) {
      this.getQueueById();
    }
  }
  initializeQueueForm() {
    return this.fb.group({
      description: [''],
      maxRetryCount: [],
      name: [''],
      id: [''],
      isDeleted: false,
      createdBy: [''],
      createdOn: [''],
      deletedBy: [''],
      deleteOn: [''],
      timestamp: [''],
      updatedOn: [''],
      updatedBy: [''],
    });
  }

  getQueueById(): void {
    this.httpService.get(`Queues/${this.urlId}`).subscribe((response) => {
      if (response) {
        response.createdOn = this.helperService.transformDate(
          response.createdOn,
          'lll'
        );
        response.updatedOn = this.helperService.transformDate(
          response.updatedOn,
          'lll'
        );
        this.viewQueueForm.patchValue({ ...response });
        this.viewQueueForm.disable();
      }
    });
  }
}
