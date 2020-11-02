import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddQueueItemsComponent } from './add-queue-items.component';

describe('AddQueueItemsComponent', () => {
  let component: AddQueueItemsComponent;
  let fixture: ComponentFixture<AddQueueItemsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddQueueItemsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddQueueItemsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
