import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SpinnerService {
  private count = 0;
  private spinner$ = new BehaviorSubject<string>('');

  constructor() {}

  getSpinnerObserver(): Observable<string> {
    return this.spinner$.asObservable();
  }

  requestStarted() {
    if (++this.count === 1) {
      // console.log(this.count, 'request Started');
      this.spinner$.next('start');
    }
  }

  requestEnded() {
    if (this.count === 0 || --this.count === 0) {
      // console.log(this.count, 'request End');
      this.spinner$.next('stop');
    }
  }

  resetSpinner() {
    this.count = 0;
    //console.log(this.count, 'Reset');
    this.spinner$.next('stop');
  }
}
