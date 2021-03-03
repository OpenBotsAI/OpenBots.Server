import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { SpinnerService } from './spinner.service';

@Component({
  selector: 'ngx-loader-spinner',
  templateUrl: './loader-spinner.component.html',
  styleUrls: ['./loader-spinner.component.scss'],
})
export class LoaderSpinnerComponent implements OnInit {
  showSpinner = false;

  constructor(
    private spinnerService: SpinnerService,
    private cdRef: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.init();
  }

  init() {
    this.spinnerService.getSpinnerObserver().subscribe((status) => {
      this.showSpinner = status === 'start';
      this.cdRef.detectChanges();
    });
  }
}
