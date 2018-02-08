import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BlobComponent } from './blob.component';

describe('BlobComponent', () => {
  let component: BlobComponent;
  let fixture: ComponentFixture<BlobComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BlobComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BlobComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
