# =====================================================================
#  Makefile cho Báo cáo Quản lý Dự án Phần mềm: IT Career Platform
#  Biên dịch chuẩn học thuật: XeLaTeX -> Biber -> XeLaTeX -> XeLaTeX
# =====================================================================

MAIN = main
TEX_ENGINE = xelatex -interaction=nonstopmode -synctex=1
BIB_ENGINE = biber

.PHONY: all build clean rebuild

all: build

build: $(MAIN).pdf

$(MAIN).pdf: $(MAIN).tex references.bib chapters/chapter1.tex chapters/chapter2.tex chapters/chapter3.tex chapters/chapter4.tex chapters/chapter5.tex chapters/chapter6.tex
	@echo "==> Lan 1: XeLaTeX..."
	$(TEX_ENGINE) $(MAIN).tex
	@echo "==> Xu ly thu vien tham khao: Biber..."
	$(BIB_ENGINE) $(MAIN)
	@echo "==> Lan 2: XeLaTeX (nap references & citations)..."
	$(TEX_ENGINE) $(MAIN).tex
	@echo "==> Lan 3: XeLaTeX (chot layout & so trang)..."
	$(TEX_ENGINE) $(MAIN).tex
	@echo "==> Bien dich thanh cong: $(MAIN).pdf"

clean:
	@echo "==> Don dep tep trung gian..."
	-cmd /c del /f /q *.aux *.bbl *.bcf *.blg *.log *.out *.run.xml *.toc *.lof *.lot *.fls *.fdb_latexmk *.synctex.gz 2>nul
	-cmd /c del /f /q chapters\*.aux 2>nul
	@echo "==> Da don dep xong."

rebuild: clean build
