import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddQueueComponent } from './queue.component';

describe('QueueComponent', () => {
  let component: AddQueueComponent;
  let fixture: ComponentFixture<AddQueueComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [AddQueueComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddQueueComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
