import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewStorageDriveComponent } from './view-storage-drive.component';

describe('ViewStorageDriveComponent', () => {
  let component: ViewStorageDriveComponent;
  let fixture: ComponentFixture<ViewStorageDriveComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ViewStorageDriveComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewStorageDriveComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
